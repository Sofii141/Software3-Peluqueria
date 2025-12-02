using Moq;
using Peluqueria.Application.Services;
using Peluqueria.Application.Interfaces;
using Peluqueria.Application.Exceptions;
using Peluqueria.Application.Dtos.Estilista;
using Peluqueria.Application.Dtos.Events;
using Peluqueria.Domain.Entities;

namespace Peluqueria.Tests.Services
{
    /// <summary>
    /// Define las pruebas unitarias exhaustivas para la clase <see cref="EstilistaAgendaService"/>.
    /// Cubre la gestión de horarios base, descansos fijos y bloqueos por vacaciones.
    /// </summary>
    public class EstilistaAgendaServiceTests
    {
        private readonly Mock<IEstilistaAgendaRepository> _agendaRepoMock;
        private readonly Mock<IEstilistaRepository> _estilistaRepoMock;
        private readonly Mock<IMessagePublisher> _messagePublisherMock;
        private readonly Mock<IReservacionClient> _reservacionClientMock;
        private readonly EstilistaAgendaService _service;

        public EstilistaAgendaServiceTests()
        {
            _agendaRepoMock = new Mock<IEstilistaAgendaRepository>();
            _estilistaRepoMock = new Mock<IEstilistaRepository>();
            _messagePublisherMock = new Mock<IMessagePublisher>();
            _reservacionClientMock = new Mock<IReservacionClient>();

            _service = new EstilistaAgendaService(
                _agendaRepoMock.Object,
                _estilistaRepoMock.Object,
                _messagePublisherMock.Object,
                _reservacionClientMock.Object
            );
        }

        #region Horario Base

        /// <summary>
        /// Verifica que <see cref="EstilistaAgendaService.UpdateHorarioBaseAsync"/> lance <see cref="EntidadNoExisteException"/>
        /// si el estilista no existe.
        /// </summary>
        [Fact]
        public async Task UpdateHorarioBase_DebeLanzarExcepcion_CuandoEstilistaNoExiste()
        {
            // Arrange
            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(99))
                .ReturnsAsync((Estilista?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntidadNoExisteException>(() =>
                _service.UpdateHorarioBaseAsync(99, new List<HorarioDiaDto>()));
        }

        /// <summary>
        /// Verifica que <see cref="EstilistaAgendaService.UpdateHorarioBaseAsync"/> lance error
        /// si la hora de inicio es mayor o igual a la hora de fin.
        /// </summary>
        [Fact]
        public async Task UpdateHorarioBase_DebeLanzarExcepcion_CuandoHorasSonInvalidas()
        {
            // Arrange
            int id = 1;
            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(id)).ReturnsAsync(new Estilista());

            var horarios = new List<HorarioDiaDto>
            {
                new HorarioDiaDto { DiaSemana = DayOfWeek.Monday, HoraInicio = new TimeSpan(18, 0, 0), HoraFin = new TimeSpan(09, 0, 0), EsLaborable = true }
            };

            // Act & Assert
            await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.UpdateHorarioBaseAsync(id, horarios));
        }

        /// <summary>
        /// Valida que <see cref="EstilistaAgendaService.UpdateHorarioBaseAsync"/> impida marcar un día como NO laborable
        /// si ya existen reservas agendadas para ese día.
        /// </summary>
        [Fact]
        public async Task UpdateHorarioBase_DebeFallar_SiIntentaQuitarDiaLaborableConReservas()
        {
            // Arrange
            int id = 1;
            var horarios = new List<HorarioDiaDto>
            {
                new HorarioDiaDto { DiaSemana = DayOfWeek.Monday, EsLaborable = false }
            };

            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(id)).ReturnsAsync(new Estilista());
            _reservacionClientMock.Setup(x => x.TieneReservasEnDia(id, DayOfWeek.Monday)).ReturnsAsync(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.UpdateHorarioBaseAsync(id, horarios));
            Assert.Equal(CodigoError.OPERACION_BLOQUEADA_POR_CITAS, ex.CodigoError);
        }

        /// <summary>
        /// Valida que <see cref="EstilistaAgendaService.UpdateHorarioBaseAsync"/> impida reducir la jornada
        /// si existen reservas que quedarían fuera del nuevo horario.
        /// </summary>
        [Fact]
        public async Task UpdateHorarioBase_DebeFallar_SiReduccionHorarioDejaCitasFuera()
        {
            // Arrange
            int id = 1;
            // Nuevo horario: 10:00 a 18:00
            var horarios = new List<HorarioDiaDto>
            {
                new HorarioDiaDto { DiaSemana = DayOfWeek.Monday, HoraInicio = new TimeSpan(10,0,0), HoraFin = new TimeSpan(18,0,0), EsLaborable = true }
            };

            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(id)).ReturnsAsync(new Estilista());

            // Simulación: Hay cita a las 09:00 AM (Conflicto en la mañana)
            _reservacionClientMock.Setup(x => x.TieneReservasEnDescanso(id, DayOfWeek.Monday, TimeSpan.Zero, new TimeSpan(10, 0, 0)))
                .ReturnsAsync(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.UpdateHorarioBaseAsync(id, horarios));
            Assert.Equal(CodigoError.OPERACION_BLOQUEADA_POR_CITAS, ex.CodigoError);
        }

        /// <summary>
        /// Verifica el flujo exitoso de actualización de horario base y la publicación del evento.
        /// </summary>
        [Fact]
        public async Task UpdateHorarioBase_DebeActualizarYPublicar_CuandoEsValido()
        {
            // Arrange
            int id = 1;
            var horarios = new List<HorarioDiaDto>
            {
                new HorarioDiaDto { DiaSemana = DayOfWeek.Monday, HoraInicio = new TimeSpan(9,0,0), HoraFin = new TimeSpan(18,0,0), EsLaborable = true }
            };

            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(id)).ReturnsAsync(new Estilista());
            _reservacionClientMock.Setup(x => x.TieneReservasEnDia(id, DayOfWeek.Monday)).ReturnsAsync(false);
            _reservacionClientMock.Setup(x => x.TieneReservasEnDescanso(It.IsAny<int>(), It.IsAny<DayOfWeek>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>())).ReturnsAsync(false);

            // Act
            var result = await _service.UpdateHorarioBaseAsync(id, horarios);

            // Assert
            Assert.True(result);
            _agendaRepoMock.Verify(x => x.UpdateHorarioBaseAsync(id, It.IsAny<List<HorarioSemanalBase>>()), Times.Once);
            _messagePublisherMock.Verify(x => x.PublishAsync(It.IsAny<HorarioBaseEstilistaEventDto>(), "horario_base.actualizado", "agenda_exchange"), Times.Once);
        }

        #endregion

        #region Descansos Fijos

        /// <summary>
        /// Verifica que <see cref="EstilistaAgendaService.UpdateDescansoFijoAsync"/> lance excepción
        /// si se intenta asignar un descanso en un día no laborable.
        /// </summary>
        [Fact]
        public async Task UpdateDescansoFijo_DebeFallar_SiDiaNoEsLaborable()
        {
            // Arrange
            int id = 1;
            var descansos = new List<HorarioDiaDto> { new HorarioDiaDto { DiaSemana = DayOfWeek.Sunday } };

            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(id)).ReturnsAsync(new Estilista());
            _agendaRepoMock.Setup(x => x.IsDiaLaborableAsync(id, DayOfWeek.Sunday)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.UpdateDescansoFijoAsync(id, descansos));
        }

        /// <summary>
        /// Verifica que <see cref="EstilistaAgendaService.UpdateDescansoFijoAsync"/> lance excepción
        /// si el nuevo descanso choca con reservas existentes.
        /// </summary>
        [Fact]
        public async Task UpdateDescansoFijo_DebeFallar_SiChocaConCitas()
        {
            // Arrange
            int id = 1;
            var descansos = new List<HorarioDiaDto>
            {
                new HorarioDiaDto { DiaSemana = DayOfWeek.Monday, HoraInicio = new TimeSpan(13,0,0), HoraFin = new TimeSpan(14,0,0) }
            };

            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(id)).ReturnsAsync(new Estilista());
            _agendaRepoMock.Setup(x => x.IsDiaLaborableAsync(id, DayOfWeek.Monday)).ReturnsAsync(true);

            // Conflicto externo
            _reservacionClientMock.Setup(x => x.TieneReservasEnDescanso(id, DayOfWeek.Monday, new TimeSpan(13, 0, 0), new TimeSpan(14, 0, 0)))
                .ReturnsAsync(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.UpdateDescansoFijoAsync(id, descansos));
            Assert.Equal(CodigoError.OPERACION_BLOQUEADA_POR_CITAS, ex.CodigoError);
        }

        /// <summary>
        /// Verifica que <see cref="EstilistaAgendaService.DeleteDescansoFijoAsync"/> elimine el descanso y publique el evento.
        /// </summary>
        [Fact]
        public async Task DeleteDescansoFijo_DebeEliminarYPublicar()
        {
            // Arrange
            int id = 1;
            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(id)).ReturnsAsync(new Estilista());

            // Act
            await _service.DeleteDescansoFijoAsync(id, DayOfWeek.Monday);

            // Assert
            _agendaRepoMock.Verify(x => x.DeleteDescansoFijoAsync(id, DayOfWeek.Monday), Times.Once);
            _messagePublisherMock.Verify(x => x.PublishAsync(It.IsAny<object>(), "descanso_fijo.eliminado", "agenda_exchange"), Times.Once);
        }

        #endregion

        #region Bloqueos (Vacaciones)

        /// <summary>
        /// Verifica que <see cref="EstilistaAgendaService.CreateBloqueoDiasLibresAsync"/> lance error si las fechas están en el pasado.
        /// </summary>
        [Fact]
        public async Task CreateBloqueo_DebeFallar_SiFechasSonPasadas()
        {
            // Arrange
            int id = 1;
            var dto = new BloqueoRangoDto { FechaInicio = DateTime.Now.AddDays(-5), FechaFin = DateTime.Now.AddDays(-2) };
            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(id)).ReturnsAsync(new Estilista());

            // Act & Assert
            await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.CreateBloqueoDiasLibresAsync(id, dto));
        }

        /// <summary>
        /// Verifica que <see cref="EstilistaAgendaService.CreateBloqueoDiasLibresAsync"/> lance error
        /// si hay solapamiento con otros bloqueos existentes (Validación Local).
        /// </summary>
        [Fact]
        public async Task CreateBloqueo_DebeFallar_SiHaySolapamientoLocal()
        {
            // Arrange
            int id = 1;
            var existente = new BloqueoRangoDiasLibres { FechaInicioBloqueo = DateTime.Today.AddDays(10), FechaFinBloqueo = DateTime.Today.AddDays(15) };
            var dto = new BloqueoRangoDto { FechaInicio = DateTime.Today.AddDays(12), FechaFin = DateTime.Today.AddDays(13) };

            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(id)).ReturnsAsync(new Estilista());
            _agendaRepoMock.Setup(x => x.GetBloqueosDiasLibresAsync(id)).ReturnsAsync(new List<BloqueoRangoDiasLibres> { existente });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.CreateBloqueoDiasLibresAsync(id, dto));
            Assert.Contains("otro bloqueo", ex.Message);
        }

        /// <summary>
        /// Verifica que <see cref="EstilistaAgendaService.CreateBloqueoDiasLibresAsync"/> lance error
        /// si hay conflicto con reservas existentes (Validación Externa).
        /// </summary>
        [Fact]
        public async Task CreateBloqueo_DebeFallar_SiHayConflictoConReservas()
        {
            // Arrange
            int id = 1;
            var dto = new BloqueoRangoDto { FechaInicio = DateTime.Today.AddDays(20), FechaFin = DateTime.Today.AddDays(25) };

            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(id)).ReturnsAsync(new Estilista());
            _agendaRepoMock.Setup(x => x.GetBloqueosDiasLibresAsync(id)).ReturnsAsync(new List<BloqueoRangoDiasLibres>());

            // Conflicto externo
            _reservacionClientMock.Setup(x => x.TieneReservasEnRango(id, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ReglaNegocioException>(() => _service.CreateBloqueoDiasLibresAsync(id, dto));
            Assert.Equal(CodigoError.OPERACION_BLOQUEADA_POR_CITAS, ex.CodigoError);
        }

        /// <summary>
        /// Verifica el flujo exitoso de creación de bloqueo y publicación del evento.
        /// </summary>
        [Fact]
        public async Task CreateBloqueo_DebeCrearYPublicar_CuandoEsValido()
        {
            // Arrange
            int id = 1;
            var dto = new BloqueoRangoDto { FechaInicio = DateTime.Today.AddDays(30), FechaFin = DateTime.Today.AddDays(35) };
            var bloqueoCreado = new BloqueoRangoDiasLibres { FechaInicioBloqueo = dto.FechaInicio, FechaFinBloqueo = dto.FechaFin };

            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(id)).ReturnsAsync(new Estilista());
            _agendaRepoMock.Setup(x => x.CreateBloqueoDiasLibresAsync(It.IsAny<BloqueoRangoDiasLibres>()))
                .ReturnsAsync(bloqueoCreado);

            // Act
            await _service.CreateBloqueoDiasLibresAsync(id, dto);

            // Assert
            _messagePublisherMock.Verify(x => x.PublishAsync(It.IsAny<BloqueoRangoDiasLibresEventDto>(), "bloqueo_dias.creado", "agenda_exchange"), Times.Once);
        }

        /// <summary>
        /// Verifica que <see cref="EstilistaAgendaService.UpdateBloqueoDiasLibresAsync"/> actualice correctamente si no hay conflictos.
        /// </summary>
        [Fact]
        public async Task UpdateBloqueo_DebeActualizar_CuandoEsValido()
        {
            // Arrange
            int id = 1;
            int bloqueoId = 10;
            var dto = new BloqueoRangoDto { FechaInicio = DateTime.Today.AddDays(40), FechaFin = DateTime.Today.AddDays(42) };

            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(id)).ReturnsAsync(new Estilista());
            _agendaRepoMock.Setup(x => x.UpdateBloqueoDiasLibresAsync(It.IsAny<BloqueoRangoDiasLibres>())).ReturnsAsync(true);

            // Act
            await _service.UpdateBloqueoDiasLibresAsync(id, bloqueoId, dto);

            // Assert
            _messagePublisherMock.Verify(x => x.PublishAsync(It.IsAny<BloqueoRangoDiasLibresEventDto>(), "bloqueo_dias.actualizado", "agenda_exchange"), Times.Once);
        }

        /// <summary>
        /// Verifica que <see cref="EstilistaAgendaService.DeleteBloqueoDiasLibresAsync"/> elimine el bloqueo.
        /// </summary>
        [Fact]
        public async Task DeleteBloqueo_DebeEliminarYPublicar()
        {
            // Arrange
            int id = 1;
            int bloqueoId = 100;
            var bloqueo = new BloqueoRangoDiasLibres { Id = bloqueoId, EstilistaId = id };

            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(id)).ReturnsAsync(new Estilista());
            _agendaRepoMock.Setup(x => x.GetBloqueosDiasLibresAsync(id)).ReturnsAsync(new List<BloqueoRangoDiasLibres> { bloqueo });

            // Act
            await _service.DeleteBloqueoDiasLibresAsync(id, bloqueoId);

            // Assert
            _agendaRepoMock.Verify(x => x.DeleteBloqueoDiasLibresAsync(bloqueoId, id), Times.Once);
            _messagePublisherMock.Verify(x => x.PublishAsync(It.IsAny<BloqueoRangoDiasLibresEventDto>(), "bloqueo_dias.eliminado", "agenda_exchange"), Times.Once);
        }

        #endregion

        #region Getters

        /// <summary>
        /// Verifica que <see cref="EstilistaAgendaService.GetHorarioBaseAsync"/> retorne los datos mapeados.
        /// </summary>
        [Fact]
        public async Task GetHorarioBase_DebeRetornarDtos()
        {
            int id = 1;
            var datos = new List<HorarioSemanalBase> { new HorarioSemanalBase { EstilistaId = id } };
            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(id)).ReturnsAsync(new Estilista());
            _agendaRepoMock.Setup(x => x.GetHorarioBaseAsync(id)).ReturnsAsync(datos);

            var result = await _service.GetHorarioBaseAsync(id);
            Assert.NotEmpty(result);
        }

        /// <summary>
        /// Verifica que <see cref="EstilistaAgendaService.GetBloqueosDiasLibresAsync"/> retorne los datos mapeados.
        /// </summary>
        [Fact]
        public async Task GetBloqueos_DebeRetornarDtos()
        {
            int id = 1;
            var datos = new List<BloqueoRangoDiasLibres> { new BloqueoRangoDiasLibres { EstilistaId = id } };
            _estilistaRepoMock.Setup(x => x.GetFullEstilistaByIdAsync(id)).ReturnsAsync(new Estilista());
            _agendaRepoMock.Setup(x => x.GetBloqueosDiasLibresAsync(id)).ReturnsAsync(datos);

            var result = await _service.GetBloqueosDiasLibresAsync(id);
            Assert.NotEmpty(result);
        }

        #endregion
    }
}