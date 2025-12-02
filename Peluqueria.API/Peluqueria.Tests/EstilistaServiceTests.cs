using Moq;
using Peluqueria.Application.Services;
using Peluqueria.Application.Interfaces;
using Peluqueria.Application.Exceptions;
using Peluqueria.Domain.Entities;
using Peluqueria.Application.Dtos.Estilista;
using Peluqueria.Application.Dtos.Events;
using Peluqueria.Application.Dtos.Account;
using Microsoft.AspNetCore.Identity;

namespace Peluqueria.Tests.Services
{
    /// <summary>
    /// Define las pruebas unitarias exhaustivas para la clase <see cref="EstilistaService"/>.
    /// Cubre CRUD completo, integración con Identity, validaciones de negocio y comunicación con microservicios.
    /// </summary>
    public class EstilistaServiceTests
    {
        private readonly Mock<IEstilistaRepository> _estilistaRepoMock;
        private readonly Mock<IMessagePublisher> _messagePublisherMock;
        private readonly Mock<IIdentityService> _identityServiceMock;
        private readonly Mock<IServicioRepository> _servicioRepoMock;
        private readonly Mock<IEstilistaAgendaService> _agendaServiceMock;
        private readonly Mock<IFileStorageService> _fileStorageMock;
        private readonly Mock<IReservacionClient> _reservacionClientMock;
        private readonly EstilistaService _service;

        public EstilistaServiceTests()
        {
            _estilistaRepoMock = new Mock<IEstilistaRepository>();
            _messagePublisherMock = new Mock<IMessagePublisher>();
            _identityServiceMock = new Mock<IIdentityService>();
            _servicioRepoMock = new Mock<IServicioRepository>();
            _agendaServiceMock = new Mock<IEstilistaAgendaService>();
            _fileStorageMock = new Mock<IFileStorageService>();
            _reservacionClientMock = new Mock<IReservacionClient>();

            _service = new EstilistaService(
                _estilistaRepoMock.Object,
                _messagePublisherMock.Object,
                _identityServiceMock.Object,
                _servicioRepoMock.Object,
                _agendaServiceMock.Object,
                _fileStorageMock.Object,
                _reservacionClientMock.Object
            );
        }

        #region GetAll y GetById

        /// <summary>
        /// Verifica que <see cref="EstilistaService.GetAllAsync"/> retorne la lista de estilistas enriquecida con datos de Identity.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_DebeRetornarListaEnriquecida()
        {
            // Arrange
            var estilistas = new List<Estilista>
            {
                new Estilista { Id = 1, IdentityId = "guid-1", NombreCompleto = "Juan" }
            };
            var userDto = new AppUserMinimalDto { Email = "juan@test.com" };

            _estilistaRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(estilistas);
            _identityServiceMock.Setup(x => x.FindByIdentityIdAsync("guid-1")).ReturnsAsync(userDto);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("juan@test.com", result.First().Email);
        }

        /// <summary>
        /// Verifica que <see cref="EstilistaService.GetByIdAsync"/> lance excepción si el estilista no existe.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_DebeLanzarExcepcion_CuandoNoExiste()
        {
            // Arrange
            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(99)).ReturnsAsync((Estilista?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntidadNoExisteException>(() => _service.GetByIdAsync(99));
        }

        /// <summary>
        /// Verifica que <see cref="EstilistaService.GetByIdAsync"/> retorne el DTO correcto si existe.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_DebeRetornarDto_CuandoExiste()
        {
            // Arrange
            var estilista = new Estilista { Id = 1, IdentityId = "guid-1" };
            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(1)).ReturnsAsync(estilista);
            _identityServiceMock.Setup(x => x.FindByIdentityIdAsync("guid-1")).ReturnsAsync(new AppUserMinimalDto());

            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        #endregion

        #region Create

        /// <summary>
        /// Verifica que <see cref="EstilistaService.CreateAsync"/> lance excepción si no se asignan servicios.
        /// </summary>
        [Fact]
        public async Task CreateAsync_DebeLanzarExcepcion_CuandoListaServiciosVacia()
        {
            // Arrange
            var req = new CreateEstilistaRequestDto { ServiciosIds = new List<int>() };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.CreateAsync(req));
            Assert.Equal(CodigoError.ESTILISTA_SIN_SERVICIOS, ex.CodigoError);
        }

        /// <summary>
        /// Verifica que <see cref="EstilistaService.CreateAsync"/> lance excepción si un servicio asignado no existe.
        /// </summary>
        [Fact]
        public async Task CreateAsync_DebeLanzarExcepcion_CuandoServicioNoExiste()
        {
            // Arrange
            var req = new CreateEstilistaRequestDto { ServiciosIds = new List<int> { 99 } };
            _servicioRepoMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Servicio?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntidadNoExisteException>(() => _service.CreateAsync(req));
        }

        /// <summary>
        /// Verifica que <see cref="EstilistaService.CreateAsync"/> lance excepción si falla la creación del usuario en Identity (ej. duplicado).
        /// </summary>
        [Fact]
        public async Task CreateAsync_DebeLanzarExcepcion_CuandoIdentityFallaPorDuplicado()
        {
            // Arrange
            var req = new CreateEstilistaRequestDto { ServiciosIds = new List<int> { 1 }, Username = "dup" };
            var error = new IdentityError { Code = "DuplicateUserName", Description = "Ya existe" };

            _servicioRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Servicio());
            _identityServiceMock.Setup(x => x.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(error));

            // Act & Assert
            await Assert.ThrowsAsync<EntidadYaExisteException>(() => _service.CreateAsync(req));
        }

        /// <summary>
        /// Verifica el flujo exitoso de <see cref="EstilistaService.CreateAsync"/>: crea usuario, asigna rol, crea entidad y publica evento.
        /// </summary>
        [Fact]
        public async Task CreateAsync_DebeCrearYPublicar_CuandoEsValido()
        {
            // Arrange
            var req = new CreateEstilistaRequestDto
            {
                ServiciosIds = new List<int> { 1 },
                Username = "new_stylist",
                Email = "s@test.com",
                NombreCompleto = "Stylist",
                Telefono = "3001234567",
                Password = "Pass"
            };

            var userDto = new AppUserMinimalDto { Id = "guid-new", Email = "s@test.com" };
            var estilistaCreado = new Estilista
            {
                Id = 10,
                IdentityId = "guid-new",
                ServiciosAsociados = new List<EstilistaServicio> { new EstilistaServicio { Servicio = new Servicio { DuracionMinutos = 60 } } }
            };

            // Mocks
            _servicioRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Servicio());
            _identityServiceMock.Setup(x => x.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _identityServiceMock.Setup(x => x.AddUserToRoleAsync(It.IsAny<string>(), "Estilista"))
                .ReturnsAsync(IdentityResult.Success);
            _identityServiceMock.Setup(x => x.FindByNameAsync(req.Username)).ReturnsAsync(userDto);

            _estilistaRepoMock.Setup(x => x.CreateAsync(It.IsAny<Estilista>(), It.IsAny<List<int>>()))
                .ReturnsAsync(estilistaCreado);
            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(10)).ReturnsAsync(estilistaCreado);

            // Act
            var result = await _service.CreateAsync(req);

            // Assert
            Assert.Equal(10, result.Id);
            _messagePublisherMock.Verify(x => x.PublishAsync(
                It.Is<EstilistaEventDto>(e => e.Id == 10 && e.Accion == "CREADO"),
                "estilista.creado",
                "estilista_exchange"),
                Times.Once);
        }

        #endregion

        #region Update

        /// <summary>
        /// Verifica que <see cref="EstilistaService.UpdateAsync"/> lance excepción si el estilista no existe.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_DebeLanzarExcepcion_CuandoEstilistaNoExiste()
        {
            // Arrange
            var req = new UpdateEstilistaRequestDto { ServiciosIds = new List<int> { 1 } };
            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(99)).ReturnsAsync((Estilista?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntidadNoExisteException>(() => _service.UpdateAsync(99, req));
        }

        /// <summary>
        /// Verifica que <see cref="EstilistaService.UpdateAsync"/> intente actualizar credenciales si se proporcionan.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_DebeActualizarCredenciales_CuandoSeProporcionan()
        {
            // Arrange
            int id = 1;
            var req = new UpdateEstilistaRequestDto { ServiciosIds = new List<int> { 1 }, Username = "newuser" };
            var estilista = new Estilista { Id = id, IdentityId = "guid-1", ServiciosAsociados = new List<EstilistaServicio>() };

            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(id)).ReturnsAsync(estilista);
            _servicioRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Servicio());
            _identityServiceMock.Setup(x => x.UpdateUserCredentialsAsync("guid-1", "newuser", ""))
                .ReturnsAsync(IdentityResult.Success);
            _identityServiceMock.Setup(x => x.FindByIdentityIdAsync("guid-1")).ReturnsAsync(new AppUserMinimalDto());

            // Act
            await _service.UpdateAsync(id, req);

            // Assert
            _identityServiceMock.Verify(x => x.UpdateUserCredentialsAsync("guid-1", "newuser", ""), Times.Once);
        }

        /// <summary>
        /// Verifica que <see cref="EstilistaService.UpdateAsync"/> lance excepción si la contraseña proporcionada es inválida.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_DebeLanzarExcepcion_CuandoPasswordEsInvalida()
        {
            // Arrange
            int id = 1;
            var req = new UpdateEstilistaRequestDto { ServiciosIds = new List<int> { 1 }, Password = "short" };
            var estilista = new Estilista { Id = id, IdentityId = "guid-1" };
            var error = new IdentityError { Description = "Password too short" };

            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(id)).ReturnsAsync(estilista);
            _servicioRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Servicio());
            _identityServiceMock.Setup(x => x.AdminResetPasswordAsync("guid-1", "short"))
                .ReturnsAsync(IdentityResult.Failed(error));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.UpdateAsync(id, req));
            Assert.Equal(CodigoError.SEGURIDAD_CUENTA, ex.CodigoError);
        }

        /// <summary>
        /// Verifica el flujo exitoso de actualización (Entidad + Evento).
        /// </summary>
        [Fact]
        public async Task UpdateAsync_DebeActualizarYPublicar_CuandoEsValido()
        {
            // Arrange
            int id = 1;
            var req = new UpdateEstilistaRequestDto { ServiciosIds = new List<int> { 1 }, NombreCompleto = "Edited" };
            var estilista = new Estilista
            {
                Id = id,
                IdentityId = "guid-1",
                ServiciosAsociados = new List<EstilistaServicio> { new EstilistaServicio { Servicio = new Servicio() } }
            };

            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(id)).ReturnsAsync(estilista);
            _servicioRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Servicio());
            _identityServiceMock.Setup(x => x.FindByIdentityIdAsync("guid-1")).ReturnsAsync(new AppUserMinimalDto());

            // Act
            await _service.UpdateAsync(id, req);

            // Assert
            _estilistaRepoMock.Verify(x => x.UpdateAsync(It.Is<Estilista>(e => e.NombreCompleto == "Edited"), It.IsAny<List<int>>()), Times.Once);
            _messagePublisherMock.Verify(x => x.PublishAsync(
                It.Is<EstilistaEventDto>(e => e.Id == id && e.Accion == "ACTUALIZADO"),
                "estilista.actualizado",
                "estilista_exchange"),
                Times.Once);
        }

        #endregion

        #region Inactivate

        /// <summary>
        /// Verifica que <see cref="EstilistaService.InactivateAsync"/> lance excepción si el estilista no existe.
        /// </summary>
        [Fact]
        public async Task InactivateAsync_DebeLanzarExcepcion_CuandoNoExiste()
        {
            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(99)).ReturnsAsync((Estilista?)null);
            await Assert.ThrowsAsync<EntidadNoExisteException>(() => _service.InactivateAsync(99));
        }

        /// <summary>
        /// Verifica que <see cref="EstilistaService.InactivateAsync"/> lance excepción si existen reservas futuras (Validación distribuida).
        /// </summary>
        [Fact]
        public async Task InactivateAsync_DebeBloquear_CuandoHayReservas()
        {
            // Arrange
            int id = 1;
            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(id)).ReturnsAsync(new Estilista { Id = id });
            _reservacionClientMock.Setup(x => x.TieneReservasEstilista(id)).ReturnsAsync(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.InactivateAsync(id));
            Assert.Equal(CodigoError.OPERACION_BLOQUEADA_POR_CITAS, ex.CodigoError);
        }

        /// <summary>
        /// Verifica que <see cref="EstilistaService.InactivateAsync"/> realice la baja lógica y publique el evento si no hay bloqueos.
        /// </summary>
        [Fact]
        public async Task InactivateAsync_DebeInactivar_CuandoNoHayReservas()
        {
            // Arrange
            int id = 1;
            var estilista = new Estilista { Id = id, EstaActivo = true, ServiciosAsociados = new List<EstilistaServicio>() };
            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(id)).ReturnsAsync(estilista);
            _reservacionClientMock.Setup(x => x.TieneReservasEstilista(id)).ReturnsAsync(false);

            // Act
            var result = await _service.InactivateAsync(id);

            // Assert
            Assert.True(result);
            Assert.False(estilista.EstaActivo);
            _estilistaRepoMock.Verify(x => x.UpdateAsync(It.Is<Estilista>(e => !e.EstaActivo), It.IsAny<List<int>>()), Times.Once);
            _messagePublisherMock.Verify(x => x.PublishAsync(
                It.Is<EstilistaEventDto>(e => e.Id == id && e.Accion == "INACTIVADO"),
                "estilista.inactivado",
                "estilista_exchange"),
                Times.Once);
        }

        #endregion
    }
}