using Moq;
using Xunit;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using peluqueria.reservaciones.Aplicacion.Fachada;
using peluqueria.reservaciones.Aplicacion.Plantilla;
using peluqueria.reservaciones.Aplicacion.Comandos;
using peluqueria.reservaciones.Aplicacion.DTO.Comunicacion;
using peluqueria.reservaciones.Core.Dominio;
using peluqueria.reservaciones.Core.Puertos.Salida;
using peluqueria.reservaciones.Core.Excepciones;

namespace peluqueria.reservaciones.Tests.Unitarios
{
    public class ReservacionManejadorTests
    {
        // Mocks de los Repositorios (Simulamos la capa de Infraestructura)
        private readonly Mock<IServicioRepositorio> _mockServicioRepo;
        private readonly Mock<IEstilistaRepositorio> _mockEstilistaRepo;
        private readonly Mock<IHorarioRepositorio> _mockHorarioRepo;
        private readonly Mock<IReservacionRepositorio> _mockReservacionRepo;

        // Sujeto de prueba (El Manejador real)
        private readonly ReservacionManejador _manejador;

        public ReservacionManejadorTests()
        {
            // 1. Inicializamos los Mocks
            _mockServicioRepo = new Mock<IServicioRepositorio>();
            _mockEstilistaRepo = new Mock<IEstilistaRepositorio>();
            _mockHorarioRepo = new Mock<IHorarioRepositorio>();
            _mockReservacionRepo = new Mock<IReservacionRepositorio>();

            // 2. Creamos las Plantillas REALES usando los Mocks
            // Esto permite probar la lógica de validación dentro de ReservacionEstandar
            var plantillaCreacion = new ReservacionEstandar(
                _mockServicioRepo.Object,
                _mockEstilistaRepo.Object,
                _mockHorarioRepo.Object,
                _mockReservacionRepo.Object
            );

            var plantillaReprogramacion = new ReservacionReprogramar(
                _mockServicioRepo.Object,
                _mockEstilistaRepo.Object,
                _mockHorarioRepo.Object,
                _mockReservacionRepo.Object
            );

            // 3. Instanciamos el Manejador inyectando las plantillas reales
            _manejador = new ReservacionManejador(
                plantillaCreacion,
                plantillaReprogramacion,
                _mockReservacionRepo.Object,
                _mockHorarioRepo.Object
            );
        }

        [Fact]
        public async Task CrearReservacion_DebeRetornarExito_CuandoDatosSonValidos()
        {
            // ARRANGE (Preparar el escenario)
            var comando = new CrearReservacionComando
            {
                ClienteIdentificacion = "123",
                ServicioId = 1,
                EstilistaId = 5,
                Fecha = new DateOnly(2025, 12, 01),
                HoraInicio = new TimeOnly(10, 0)
            };

            // Simulamos que el Servicio existe y está disponible
            _mockServicioRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new Servicio { Id = 1, Nombre = "Corte", Disponible = true, DuracionMinutos = 30 });

            // Simulamos que el Estilista existe y está activo
            _mockEstilistaRepo.Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(new Estilista { Id = 5, NombreCompleto = "Juan", EstaActivo = true });

            // Simulamos un horario base válido (Trabaja Lunes a Viernes de 8 a 17)
            var horarioBase = new HorarioBase
            {
                HorariosSemanales = new List<DiaHorario> {
                    new DiaHorario { DiaSemana = DayOfWeek.Monday, EsLaborable = true, HoraInicio = new TimeSpan(8,0,0), HoraFin = new TimeSpan(17,0,0) }
                }
            };
            _mockHorarioRepo.Setup(r => r.GetStylistScheduleAsync(5)).ReturnsAsync(horarioBase);

            // Simulamos que NO hay conflictos de citas (lista vacía)
            _mockReservacionRepo.Setup(r => r.ObtenerReservasConflictivasAsync(5, It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>()))
                .ReturnsAsync(new List<Reservacion>());

            // Simulamos el Guardado (devolvemos la reserva con ID generado)
            _mockReservacionRepo.Setup(r => r.GuardarAsync(It.IsAny<Reservacion>()))
                .ReturnsAsync((Reservacion r) => { r.Id = 100; return r; }); // Simulamos que la BD asignó ID 100

            // ACT (Ejecutar)
            var resultado = await _manejador.CrearReservacionAsync(comando);

            // ASSERT (Verificar)
            Assert.NotNull(resultado);
            Assert.Equal(100, resultado.ReservacionId); // Verificamos que devolvió el ID simulado
            Assert.Equal("PENDIENTE", resultado.Estado); // Verificamos que la plantilla asignó el estado

            // Verificamos que se llamó al repositorio para guardar
            _mockReservacionRepo.Verify(r => r.GuardarAsync(It.IsAny<Reservacion>()), Times.Once);
        }

        [Fact]
        public async Task CrearReservacion_DebeLanzarExcepcion_CuandoServicioNoExiste()
        {
            // ARRANGE
            var comando = new CrearReservacionComando { ServicioId = 99, EstilistaId = 5, ClienteIdentificacion = "123", Fecha = DateOnly.FromDateTime(DateTime.Now), HoraInicio = new TimeOnly(10, 0) };

            // Simulamos que el servicio devuelve NULL
            _mockServicioRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Servicio?)null);

            // ACT & ASSERT
            // Esperamos que el manejador (a través de la plantilla) lance una excepción de validación
            await Assert.ThrowsAsync<ValidacionDatosExeption>(() =>
                _manejador.CrearReservacionAsync(comando));
        }

        [Fact]
        public async Task ReprogramarReservacion_DebeActualizar_CuandoHayDisponibilidad()
        {
            // ARRANGE
            int reservacionId = 10;
            var peticion = new ReservacionPeticionDTO
            {
                Fecha = new DateOnly(2025, 12, 02),
                HoraInicio = new TimeOnly(14, 0)
            };

            // Reserva existente en BD
            var reservaExistente = new Reservacion
            {
                Id = reservacionId,
                ServicioId = 1,
                EstilistaId = 5,
                Estado = "CONFIRMADA",
                Servicio = new Servicio { Id = 1, DuracionMinutos = 30 } // Necesario para recalcular tiempo
            };

            _mockReservacionRepo.Setup(r => r.ObtenerPorIdAsync(reservacionId)).ReturnsAsync(reservaExistente);

            // Setup de dependencias para validar (Servicio, Estilista, Horario)...
            // Nota: Para ahorrar espacio, asumo mocks similares al primer test
            _mockServicioRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Servicio { Id = 1, Disponible = true, DuracionMinutos = 30 });
            _mockEstilistaRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Estilista { Id = 5, EstaActivo = true });

            var horarioBase = new HorarioBase { HorariosSemanales = new List<DiaHorario> { new DiaHorario { DiaSemana = DayOfWeek.Tuesday, EsLaborable = true, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(18, 0, 0) } } };
            _mockHorarioRepo.Setup(r => r.GetStylistScheduleAsync(5)).ReturnsAsync(horarioBase);
            _mockReservacionRepo.Setup(r => r.ObtenerReservasConflictivasAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>())).ReturnsAsync(new List<Reservacion>());

            // ACT
            var resultado = await _manejador.ReprogramarReservacionAsync(reservacionId, peticion);

            // ASSERT
            Assert.Equal(new DateOnly(2025, 12, 02), resultado.Fecha);
            Assert.Equal(new TimeOnly(14, 0), resultado.HoraInicio);

            // Verificamos que se llamó a ActualizarAsync en lugar de GuardarAsync
            _mockReservacionRepo.Verify(r => r.ActualizarAsync(It.IsAny<Reservacion>()), Times.Once);
        }

        [Fact]
        public async Task ConsultarReservacionesCliente_DebeRetornarListaDTOs()
        {
            // ARRANGE
            var clienteIdentificacion = "456";
            var listaReservacionesDominio = new List<Reservacion>
    {
        new Reservacion { Id = 1, ClienteIdentificacion = clienteIdentificacion, Estado = "PENDIENTE" },
        new Reservacion { Id = 2, ClienteIdentificacion = clienteIdentificacion, Estado = "CONFIRMADA" }
    };

            // Simulamos que el repositorio devuelve la lista de entidades
            _mockReservacionRepo.Setup(r => r.BuscarReservasPorClienteAsync(clienteIdentificacion))
                .ReturnsAsync(listaReservacionesDominio);

            // ACT
            var resultado = await _manejador.ConsultarReservacionesClienteAsync(clienteIdentificacion);

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count);
            // Corregido: Usar 'NombreIdentificacion' del DTO
            Assert.All(resultado, dto => Assert.Equal(clienteIdentificacion, dto.NombreIdentificacion));

            _mockReservacionRepo.Verify(r => r.BuscarReservasPorClienteAsync(clienteIdentificacion), Times.Once);
        }

        [Fact]
        public async Task ConsultarReservasEstilistaFecha_DebeRetornarListaDTOs_PorEstilistaYFecha()
        {
            // ARRANGE
            var peticion = new PeticionReservaEstilistaFechaDTO { EstilistaId = 5, Fecha = new DateOnly(2025, 12, 10) };
            var listaReservacionesDominio = new List<Reservacion>
    {
        new Reservacion { Id = 3, EstilistaId = 5, Fecha = peticion.Fecha },
        new Reservacion { Id = 4, EstilistaId = 5, Fecha = peticion.Fecha }
    };

            // Simulamos la respuesta del repositorio
            _mockReservacionRepo.Setup(r => r.BuscarReservasPorEstilistaAsync(peticion.EstilistaId, peticion.Fecha))
                .ReturnsAsync(listaReservacionesDominio);

            // ACT
            var resultado = await _manejador.ConsultarReservasEstilistaFechaAsync(peticion);

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count);
            Assert.All(resultado, dto => Assert.Equal(peticion.EstilistaId, dto.EstilistaId));

            _mockReservacionRepo.Verify(r => r.BuscarReservasPorEstilistaAsync(peticion.EstilistaId, peticion.Fecha), Times.Once);
        }

        [Fact]
        public async Task ConsultarReservasEstilistaRango_DebeRetornarListaDTOs_PorRango()
        {
            // ARRANGE
            var peticion = new PeticionReservasEstilistaDTO
            {
                EstilistaId = 5,
                FechaInicio = new DateOnly(2025, 12, 01),
                FechaFin = new DateOnly(2025, 12, 31)
            };
            var listaReservacionesDominio = new List<Reservacion>
    {
        new Reservacion { Id = 5, EstilistaId = 5, Fecha = peticion.FechaInicio.AddDays(5) }
    };

            // Simulamos la respuesta del repositorio
            _mockReservacionRepo.Setup(r => r.BuscarReservasEstilistaRangoAsync(
                peticion.EstilistaId, peticion.FechaInicio, peticion.FechaFin))
                .ReturnsAsync(listaReservacionesDominio);

            // ACT
            var resultado = await _manejador.ConsultarReservasEstilistaRangoAsync(peticion);

            // ASSERT
            Assert.NotNull(resultado);
            Assert.Single(resultado); // Solo hay un resultado
            _mockReservacionRepo.Verify(r => r.BuscarReservasEstilistaRangoAsync(
                peticion.EstilistaId, peticion.FechaInicio, peticion.FechaFin), Times.Once);
        }

        [Fact]
        public async Task CambiarEstadoReservacion_DebeLlamarAlRepositorio_CuandoReservacionExiste()
        {
            // ARRANGE
            var peticion = new CambioEstadoDTO { ReservacionId = 10, NuevoEstado = "CANCELADA" };

            // Simulamos que la reserva existe
            _mockReservacionRepo.Setup(r => r.ObtenerPorIdAsync(peticion.ReservacionId))
                .ReturnsAsync(new Reservacion { Id = 10, Estado = "PENDIENTE" });

            // Simulamos que el cambio de estado será exitoso
            _mockReservacionRepo.Setup(r => r.CambiarEstadoAsync(peticion.ReservacionId, peticion.NuevoEstado))
                .Returns(Task.CompletedTask);

            // ACT
            await _manejador.CambiarEstadoReservacionAsync(peticion);

            // ASSERT
            // Corregido: 'picion' cambiado a 'peticion'
            _mockReservacionRepo.Verify(r => r.ObtenerPorIdAsync(peticion.ReservacionId), Times.Once);
            _mockReservacionRepo.Verify(r => r.CambiarEstadoAsync(peticion.ReservacionId, peticion.NuevoEstado), Times.Once);
        }

        [Fact]
        public async Task CambiarEstadoReservacion_DebeLanzarExcepcion_CuandoReservacionNoExiste()
        {
            // ARRANGE
            var peticion = new CambioEstadoDTO { ReservacionId = 999, NuevoEstado = "CANCELADA" };

            // Simulamos que la reserva NO existe (devuelve null)
            _mockReservacionRepo.Setup(r => r.ObtenerPorIdAsync(peticion.ReservacionId))
                .ReturnsAsync((Reservacion?)null);

            // ACT & ASSERT
            await Assert.ThrowsAsync<ValidacionDatosExeption>(() =>
                _manejador.CambiarEstadoReservacionAsync(peticion));

            // Verificamos que, como falló la búsqueda, *NO* se llamó a CambiarEstadoAsync
            _mockReservacionRepo.Verify(r => r.CambiarEstadoAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CancelarReservacion_DebeLlamarACambiarEstadoConEstadoCancelada()
        {
            // ARRANGE
            // 1. Definimos los datos de la petición de cancelación
            int reservacionId = 20;
            string estadoCancelado = "CANCELADA";
            var peticion = new CambioEstadoDTO { ReservacionId = reservacionId, NuevoEstado = estadoCancelado };

            // 2. Simular que la reserva existe y está activa (por ejemplo, 'CONFIRMADA')
            _mockReservacionRepo.Setup(r => r.ObtenerPorIdAsync(reservacionId))
                .ReturnsAsync(new Reservacion { Id = reservacionId, Estado = "CONFIRMADA" });

            // 3. Simular que el repositorio acepta el cambio de estado
            _mockReservacionRepo.Setup(r => r.CambiarEstadoAsync(reservacionId, estadoCancelado))
                .Returns(Task.CompletedTask);

            // ACT
            // El manejador llama al método de cambio de estado
            await _manejador.CambiarEstadoReservacionAsync(peticion);

            // ASSERT
            // Verificar que se intentó obtener la reserva primero
            _mockReservacionRepo.Verify(r => r.ObtenerPorIdAsync(reservacionId), Times.Once);

            // Verificar que se llamó *exactamente una vez* al método de cambio de estado
            // y que se le pasó el estado "CANCELADA".
            _mockReservacionRepo.Verify(r => r.CambiarEstadoAsync(reservacionId, estadoCancelado), Times.Once);
        }

    }
}