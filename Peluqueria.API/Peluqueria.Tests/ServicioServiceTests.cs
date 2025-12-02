using Moq;
using Xunit;
using Peluqueria.Application.Services;
using Peluqueria.Application.Interfaces;
using Peluqueria.Application.Exceptions;
using Peluqueria.Application.Dtos.Servicio;
using Peluqueria.Application.Dtos.Events;
using Peluqueria.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Peluqueria.Tests.Services
{
    /// <summary>
    /// Define las pruebas unitarias exhaustivas para la clase <see cref="ServicioService"/>.
    /// Cubre reglas de negocio (duración, precio, imágenes), CRUD y validaciones distribuidas.
    /// </summary>
    public class ServicioServiceTests
    {
        private readonly Mock<IServicioRepository> _servicioRepoMock;
        private readonly Mock<IFileStorageService> _fileStorageMock;
        private readonly Mock<ICategoriaRepository> _categoriaRepoMock;
        private readonly Mock<IMessagePublisher> _messagePublisherMock;
        private readonly Mock<IReservacionClient> _reservacionClientMock;
        private readonly ServicioService _service;

        public ServicioServiceTests()
        {
            _servicioRepoMock = new Mock<IServicioRepository>();
            _fileStorageMock = new Mock<IFileStorageService>();
            _categoriaRepoMock = new Mock<ICategoriaRepository>();
            _messagePublisherMock = new Mock<IMessagePublisher>();
            _reservacionClientMock = new Mock<IReservacionClient>();

            _service = new ServicioService(
                _servicioRepoMock.Object,
                _fileStorageMock.Object,
                _categoriaRepoMock.Object,
                _messagePublisherMock.Object,
                _reservacionClientMock.Object
            );
        }

        #region Create

        /// <summary>
        /// Verifica que <see cref="ServicioService.CreateAsync"/> lance excepción si la duración es menor a 45 minutos.
        /// </summary>
        [Theory]
        [InlineData(10)]
        [InlineData(44)]
        public async Task CreateAsync_DebeLanzarError_CuandoDuracionEsMenorAlMinimo(int duracion)
        {
            // Arrange
            var req = new CreateServicioRequestDto { DuracionMinutos = duracion };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.CreateAsync(req));
            Assert.Contains("45 minutos", ex.Message);
        }

        /// <summary>
        /// Verifica que <see cref="ServicioService.CreateAsync"/> lance excepción si la duración excede los 480 minutos.
        /// </summary>
        [Theory]
        [InlineData(481)]
        [InlineData(600)]
        public async Task CreateAsync_DebeLanzarError_CuandoDuracionEsMayorAlMaximo(int duracion)
        {
            // Arrange
            var req = new CreateServicioRequestDto { DuracionMinutos = duracion };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.CreateAsync(req));
            Assert.Contains("480 minutos", ex.Message);
        }

        /// <summary>
        /// Verifica que <see cref="ServicioService.CreateAsync"/> lance excepción si la categoría no existe.
        /// </summary>
        [Fact]
        public async Task CreateAsync_DebeLanzarExcepcion_CuandoCategoriaNoExiste()
        {
            // Arrange
            var req = new CreateServicioRequestDto { DuracionMinutos = 60, CategoriaId = 99 };
            _categoriaRepoMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Categoria?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntidadNoExisteException>(() => _service.CreateAsync(req));
        }

        /// <summary>
        /// Verifica que <see cref="ServicioService.CreateAsync"/> lance excepción si el nombre del servicio ya existe.
        /// </summary>
        [Fact]
        public async Task CreateAsync_DebeLanzarExcepcion_CuandoNombreDuplicado()
        {
            // Arrange
            var req = new CreateServicioRequestDto { DuracionMinutos = 60, CategoriaId = 1, Nombre = "CorteX" };

            _categoriaRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Categoria());
            _servicioRepoMock.Setup(x => x.ExistsByNameAsync("CorteX")).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<EntidadYaExisteException>(() => _service.CreateAsync(req));
        }

        /// <summary>
        /// Verifica que <see cref="ServicioService.CreateAsync"/> lance excepción si el precio no tiene formato válido.
        /// </summary>
        [Theory]
        [InlineData("gratis")]
        [InlineData("-100")]
        public async Task CreateAsync_DebeLanzarExcepcion_CuandoPrecioEsInvalido(string precio)
        {
            // Arrange
            var req = new CreateServicioRequestDto { DuracionMinutos = 60, CategoriaId = 1, Nombre = "Corte", Precio = precio };

            _categoriaRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Categoria());
            _servicioRepoMock.Setup(x => x.ExistsByNameAsync("Corte")).ReturnsAsync(false);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.CreateAsync(req));
            Assert.Equal(CodigoError.PRECIO_INVALIDO, ex.CodigoError);
        }

        /// <summary>
        /// Verifica que <see cref="ServicioService.CreateAsync"/> lance excepción si la imagen es demasiado grande o tipo incorrecto.
        /// </summary>
        [Fact]
        public async Task CreateAsync_DebeLanzarExcepcion_CuandoImagenEsInvalida()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(10 * 1024 * 1024); // 10MB (Límite 5MB)
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");

            var req = new CreateServicioRequestDto { DuracionMinutos = 60, Imagen = fileMock.Object };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.CreateAsync(req));
            Assert.Equal(CodigoError.IMAGEN_INVALIDA, ex.CodigoError);
        }

        /// <summary>
        /// Verifica el flujo exitoso de creación, guardado de imagen y publicación de evento.
        /// </summary>
        [Fact]
        public async Task CreateAsync_DebeCrearYPublicar_CuandoEsValido()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.ContentType).Returns("image/png");

            var req = new CreateServicioRequestDto
            {
                DuracionMinutos = 60,
                CategoriaId = 1,
                Nombre = "Nuevo",
                Precio = "50000",
                Imagen = fileMock.Object
            };

            var servicioCreado = new Servicio { Id = 10, Nombre = "Nuevo", Categoria = new Categoria() };

            _categoriaRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Categoria());
            _servicioRepoMock.Setup(x => x.ExistsByNameAsync("Nuevo")).ReturnsAsync(false);
            _fileStorageMock.Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), "images")).ReturnsAsync("img.png");

            _servicioRepoMock.Setup(x => x.CreateAsync(It.IsAny<Servicio>())).ReturnsAsync(servicioCreado);
            _servicioRepoMock.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(servicioCreado); // Retorno para el map final

            // Act
            var res = await _service.CreateAsync(req);

            // Assert
            Assert.Equal(10, res.Id);
            _messagePublisherMock.Verify(x => x.PublishAsync(
                It.Is<ServicioEventDto>(e => e.Id == 10 && e.Accion == "CREADO"),
                "servicio.creado",
                "servicio_exchange"),
                Times.Once);
        }

        #endregion

        #region Update

        /// <summary>
        /// Verifica que <see cref="ServicioService.UpdateAsync"/> lance excepción si el servicio no existe.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_DebeLanzarExcepcion_CuandoNoExiste()
        {
            _servicioRepoMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Servicio?)null);
            await Assert.ThrowsAsync<EntidadNoExisteException>(() => _service.UpdateAsync(99, new UpdateServicioRequestDto()));
        }

        /// <summary>
        /// Verifica que <see cref="ServicioService.UpdateAsync"/> bloquee la actualización si hay reservas futuras.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_DebeBloquear_CuandoHayReservas()
        {
            // Arrange
            _servicioRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Servicio());
            _reservacionClientMock.Setup(x => x.TieneReservasServicio(1)).ReturnsAsync(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.UpdateAsync(1, new UpdateServicioRequestDto()));
            Assert.Equal(CodigoError.SERVICIO_BLOQUEADO_POR_CITAS, ex.CodigoError);
        }

        /// <summary>
        /// Verifica el flujo exitoso de actualización y publicación de evento.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_DebeActualizarYPublicar_CuandoEsValido()
        {
            // Arrange
            var req = new UpdateServicioRequestDto { DuracionMinutos = 60, Precio = "20000", CategoriaId = 1, Nombre = "Editado" };
            var servicio = new Servicio { Id = 1, Nombre = "Original" };

            _servicioRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(servicio);
            _reservacionClientMock.Setup(x => x.TieneReservasServicio(1)).ReturnsAsync(false);
            _categoriaRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Categoria());

            _servicioRepoMock.Setup(x => x.UpdateAsync(1, It.IsAny<Servicio>())).ReturnsAsync(servicio);

            // Act
            await _service.UpdateAsync(1, req);

            // Assert
            Assert.Equal("Editado", servicio.Nombre); // Verifica cambio en memoria
            _messagePublisherMock.Verify(x => x.PublishAsync(
                It.Is<ServicioEventDto>(e => e.Id == 1 && e.Accion == "ACTUALIZADO"),
                "servicio.actualizado",
                "servicio_exchange"),
                Times.Once);
        }

        #endregion

        #region Inactivate

        /// <summary>
        /// Verifica que <see cref="ServicioService.InactivateAsync"/> lance excepción si el servicio no existe.
        /// </summary>
        [Fact]
        public async Task InactivateAsync_DebeLanzarExcepcion_CuandoNoExiste()
        {
            _servicioRepoMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Servicio?)null);
            await Assert.ThrowsAsync<EntidadNoExisteException>(() => _service.InactivateAsync(99));
        }

        /// <summary>
        /// Verifica que <see cref="ServicioService.InactivateAsync"/> bloquee la acción si hay reservas futuras.
        /// </summary>
        [Fact]
        public async Task InactivateAsync_DebeBloquear_CuandoHayReservas()
        {
            _servicioRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Servicio());
            _reservacionClientMock.Setup(x => x.TieneReservasServicio(1)).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.InactivateAsync(1));
            Assert.Equal(CodigoError.SERVICIO_BLOQUEADO_POR_CITAS, ex.CodigoError);
        }

        /// <summary>
        /// Verifica el flujo exitoso de inactivación y publicación de evento.
        /// </summary>
        [Fact]
        public async Task InactivateAsync_DebeInactivar_CuandoNoHayReservas()
        {
            // Arrange
            var servicio = new Servicio { Id = 1, Disponible = true };
            _servicioRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(servicio);
            _reservacionClientMock.Setup(x => x.TieneReservasServicio(1)).ReturnsAsync(false);
            _servicioRepoMock.Setup(x => x.InactivateAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _service.InactivateAsync(1);

            // Assert
            Assert.True(result);
            Assert.False(servicio.Disponible);
            _messagePublisherMock.Verify(x => x.PublishAsync(
                It.Is<ServicioEventDto>(e => e.Id == 1 && e.Accion == "INACTIVADO"),
                "servicio.inactivado",
                "servicio_exchange"),
                Times.Once);
        }

        #endregion

        #region Getters

        /// <summary>
        /// Verifica que <see cref="ServicioService.GetAllAsync"/> retorne DTOs.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_DebeRetornarLista()
        {
            var lista = new List<Servicio> { new Servicio { Id = 1, Categoria = new Categoria() } };
            _servicioRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(lista);

            var result = await _service.GetAllAsync();
            Assert.Single(result);
        }

        /// <summary>
        /// Verifica que <see cref="ServicioService.GetByIdAsync"/> lance excepción si no existe.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_DebeLanzarExcepcion_CuandoNoExiste()
        {
            _servicioRepoMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Servicio?)null);
            await Assert.ThrowsAsync<EntidadNoExisteException>(() => _service.GetByIdAsync(99));
        }

        /// <summary>
        /// Verifica que <see cref="ServicioService.GetByCategoriaIdAsync"/> lance excepción si la categoría no existe.
        /// </summary>
        [Fact]
        public async Task GetByCategoriaIdAsync_DebeLanzarExcepcion_CuandoCategoriaNoExiste()
        {
            _categoriaRepoMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Categoria?)null);
            await Assert.ThrowsAsync<EntidadNoExisteException>(() => _service.GetByCategoriaIdAsync(99));
        }

        /// <summary>
        /// Verifica que <see cref="ServicioService.GetByCategoriaIdAsync"/> retorne servicios si la categoría existe.
        /// </summary>
        [Fact]
        public async Task GetByCategoriaIdAsync_DebeRetornarLista_CuandoExiste()
        {
            _categoriaRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Categoria());
            _servicioRepoMock.Setup(x => x.GetByCategoriaIdAsync(1))
                .ReturnsAsync(new List<Servicio> { new Servicio { Categoria = new Categoria() } });

            var result = await _service.GetByCategoriaIdAsync(1);
            Assert.Single(result);
        }

        #endregion
    }
}