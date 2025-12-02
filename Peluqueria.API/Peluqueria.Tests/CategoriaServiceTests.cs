using Moq;
using Peluqueria.Application.Services;
using Peluqueria.Application.Interfaces;
using Peluqueria.Application.Exceptions;
using Peluqueria.Domain.Entities;
using Peluqueria.Application.Dtos.Categoria;
using Peluqueria.Application.Dtos.Events;

namespace Peluqueria.Tests.Services
{
    /// <summary>
    /// Define las pruebas unitarias exhaustivas para la clase <see cref="CategoriaService"/>.
    /// Cubre operaciones CRUD, validaciones de negocio y comunicación con microservicios.
    /// </summary>
    public class CategoriaServiceTests
    {
        private readonly Mock<ICategoriaRepository> _categoriaRepoMock;
        private readonly Mock<IServicioRepository> _servicioRepoMock;
        private readonly Mock<IMessagePublisher> _messagePublisherMock;
        private readonly Mock<IReservacionClient> _reservacionClientMock;
        private readonly CategoriaService _service;

        public CategoriaServiceTests()
        {
            _categoriaRepoMock = new Mock<ICategoriaRepository>();
            _servicioRepoMock = new Mock<IServicioRepository>();
            _messagePublisherMock = new Mock<IMessagePublisher>();
            _reservacionClientMock = new Mock<IReservacionClient>();

            _service = new CategoriaService(
                _categoriaRepoMock.Object,
                _servicioRepoMock.Object,
                _messagePublisherMock.Object,
                _reservacionClientMock.Object
            );
        }

        /// <summary>
        /// Verifica que el método <see cref="CategoriaService.GetAllAsync"/> retorne correctamente la lista de DTOs mapeados.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_DebeRetornarListaDeCategorias()
        {
            // Arrange
            var listaCategorias = new List<Categoria>
            {
                new Categoria { Id = 1, Nombre = "Corte", EstaActiva = true },
                new Categoria { Id = 2, Nombre = "Color", EstaActiva = true }
            };

            _categoriaRepoMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(listaCategorias);

            // Act
            var resultado = await _service.GetAllAsync();

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count());
            Assert.Equal("Corte", resultado.First().Nombre);
        }

        /// <summary>
        /// Verifica que <see cref="CategoriaService.CreateAsync"/> lance <see cref="EntidadYaExisteException"/>
        /// cuando se intenta crear una categoría con un nombre que ya existe en la base de datos.
        /// </summary>
        [Fact]
        public async Task CreateAsync_DebeLanzarExcepcion_CuandoNombreYaExiste()
        {
            // Arrange
            var requestDto = new CreateCategoriaRequestDto { Nombre = "Existente" };

            _categoriaRepoMock.Setup(x => x.GetByNameAsync("Existente"))
                .ReturnsAsync(new Categoria { Id = 1, Nombre = "Existente" });

            // Act & Assert
            await Assert.ThrowsAsync<EntidadYaExisteException>(() => _service.CreateAsync(requestDto));
        }

        /// <summary>
        /// Verifica que <see cref="CategoriaService.CreateAsync"/> cree la categoría, persista en la base de datos
        /// y publique el evento "CREADA" en el bus de mensajes cuando los datos son válidos.
        /// </summary>
        [Fact]
        public async Task CreateAsync_DebeCrearYPublicarEvento_CuandoDatosSonValidos()
        {
            // Arrange
            var requestDto = new CreateCategoriaRequestDto { Nombre = "Nueva Categoria" };
            var categoriaCreada = new Categoria { Id = 10, Nombre = "Nueva Categoria", EstaActiva = true };

            _categoriaRepoMock.Setup(x => x.GetByNameAsync(requestDto.Nombre))
                .ReturnsAsync((Categoria?)null);

            _categoriaRepoMock.Setup(x => x.CreateAsync(It.IsAny<Categoria>()))
                .ReturnsAsync(categoriaCreada);

            // Act
            var resultado = await _service.CreateAsync(requestDto);

            // Assert
            Assert.Equal(10, resultado.Id);
            Assert.Equal("Nueva Categoria", resultado.Nombre);

            _messagePublisherMock.Verify(x => x.PublishAsync(
                It.Is<CategoriaEventDto>(e => e.Id == 10 && e.Accion == "CREADA"),
                "categoria.creada",
                "categoria_exchange"),
                Times.Once);
        }

        /// <summary>
        /// Verifica que <see cref="CategoriaService.UpdateAsync"/> lance <see cref="EntidadNoExisteException"/>
        /// si se intenta actualizar un ID que no existe.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_DebeLanzarExcepcion_CuandoIdNoExiste()
        {
            // Arrange
            int idInexistente = 99;
            _categoriaRepoMock.Setup(x => x.GetByIdAsync(idInexistente))
                .ReturnsAsync((Categoria?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntidadNoExisteException>(() =>
                _service.UpdateAsync(idInexistente, new UpdateCategoriaRequestDto { Nombre = "Test" }));
        }

        /// <summary>
        /// Verifica que <see cref="CategoriaService.UpdateAsync"/> lance <see cref="EntidadYaExisteException"/>
        /// si el nuevo nombre ya está siendo usado por OTRA categoría diferente.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_DebeLanzarExcepcion_CuandoNombreDuplicadoEnOtraCategoria()
        {
            // Arrange
            int idActualizar = 1;
            var requestDto = new UpdateCategoriaRequestDto { Nombre = "NombreDuplicado" };
            var categoriaActual = new Categoria { Id = idActualizar, Nombre = "Original" };
            var otraCategoria = new Categoria { Id = 2, Nombre = "NombreDuplicado" }; // Conflicto: ID distinto

            _categoriaRepoMock.Setup(x => x.GetByIdAsync(idActualizar))
                .ReturnsAsync(categoriaActual);

            _categoriaRepoMock.Setup(x => x.GetByNameAsync("NombreDuplicado"))
                .ReturnsAsync(otraCategoria);

            // Act & Assert
            await Assert.ThrowsAsync<EntidadYaExisteException>(() => _service.UpdateAsync(idActualizar, requestDto));
        }

        /// <summary>
        /// Verifica que <see cref="CategoriaService.UpdateAsync"/> actualice la entidad y publique el evento "ACTUALIZADA"
        /// cuando las validaciones son exitosas.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_DebeActualizarYPublicar_CuandoEsValido()
        {
            // Arrange
            int id = 1;
            var requestDto = new UpdateCategoriaRequestDto { Nombre = "Editado", EstaActiva = true };
            var categoria = new Categoria { Id = 1, Nombre = "Original" };

            _categoriaRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(categoria);

            _categoriaRepoMock.Setup(x => x.GetByNameAsync("Editado"))
                .ReturnsAsync((Categoria?)null); // No existe conflicto

            _categoriaRepoMock.Setup(x => x.UpdateAsync(id, It.IsAny<Categoria>()))
                .ReturnsAsync(categoria);

            // Act
            var resultado = await _service.UpdateAsync(id, requestDto);

            // Assert
            Assert.Equal("Editado", resultado.Nombre);

            _messagePublisherMock.Verify(x => x.PublishAsync(
                It.Is<CategoriaEventDto>(e => e.Id == id && e.Accion == "ACTUALIZADA"),
                "categoria.actualizada",
                "categoria_exchange"),
                Times.Once);
        }

        /// <summary>
        /// Verifica que <see cref="CategoriaService.InactivateAsync"/> lance <see cref="EntidadNoExisteException"/>
        /// si la categoría a inactivar no existe.
        /// </summary>
        [Fact]
        public async Task InactivateAsync_DebeLanzarExcepcion_CuandoCategoriaNoExiste()
        {
            // Arrange
            _categoriaRepoMock.Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((Categoria?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntidadNoExisteException>(() => _service.InactivateAsync(99));
        }

        /// <summary>
        /// Verifica que <see cref="CategoriaService.InactivateAsync"/> lance <see cref="ReglaNegocioException"/>
        /// cuando el microservicio de reservas indica que existen reservas futuras asociadas a esta categoría.
        /// </summary>
        [Fact]
        public async Task InactivateAsync_DebeBloquearAccion_CuandoExistenReservasAsociadas()
        {
            // Arrange
            int id = 5;
            _categoriaRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(new Categoria { Id = id, EstaActiva = true });

            // Simulación: El cliente HTTP retorna TRUE (Conflicto)
            _reservacionClientMock.Setup(x => x.TieneReservasCategoria(id))
                .ReturnsAsync(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.InactivateAsync(id));
            Assert.Equal(CodigoError.CATEGORIA_CON_SERVICIOS, ex.CodigoError);
        }

        /// <summary>
        /// Verifica que <see cref="CategoriaService.InactivateAsync"/> realice la baja lógica y publique el evento "INACTIVADA"
        /// cuando no existen bloqueos externos.
        /// </summary>
        [Fact]
        public async Task InactivateAsync_DebeInactivar_CuandoNoTieneReservas()
        {
            // Arrange
            int id = 1;
            var categoria = new Categoria { Id = id, EstaActiva = true };

            _categoriaRepoMock.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(categoria);

            // Simulación: El cliente HTTP retorna FALSE (No hay conflicto)
            _reservacionClientMock.Setup(x => x.TieneReservasCategoria(id))
                .ReturnsAsync(false);

            _categoriaRepoMock.Setup(x => x.InactivateAsync(id))
                .ReturnsAsync(true);

            // Act
            var resultado = await _service.InactivateAsync(id);

            // Assert
            Assert.True(resultado);
            Assert.False(categoria.EstaActiva); // Verifica cambio de estado en memoria

            _messagePublisherMock.Verify(x => x.PublishAsync(
                It.Is<CategoriaEventDto>(e => e.Id == id && e.Accion == "INACTIVADA"),
                "categoria.inactivada",
                "categoria_exchange"),
                Times.Once);
        }
    }
}