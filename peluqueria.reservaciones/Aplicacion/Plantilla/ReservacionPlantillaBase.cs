using peluqueria.reservaciones.Core.Dominio;
using System.Threading.Tasks;
using peluqueria.reservaciones.Core.Dominio;
using peluqueria.reservaciones.Core.Excepciones;
using peluqueria.reservaciones.Core.Puertos.Salida;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace peluqueria.reservaciones.Aplicacion.Plantilla
{
    public abstract class ReservacionPlantillaBase
    {
        // Validar los datos de la reservacion para evitar nulos
        public abstract Task<Reservacion> ValidarDatosAsync(Reservacion reservacion);

        // Verificar que e servicio y estilista existan
        public abstract Task<Reservacion> ValidarServicioEstilistaAsync(Reservacion reservacion);

        // Calcular el tiempo de atencion total de la reservacion
        public abstract Task<Reservacion> CalcularTiempoAtencionAsync(Reservacion reservacion);

        // Consultar la disponibilidad del estilista en el horario solicitado
        public abstract Task<Reservacion> ValidarDisponibilidadAsync(Reservacion reservacion);

        //Guardar la reservacion en el repositorio
        public abstract Task<Reservacion> PersistirReservacionAsync(Reservacion reservacion);

        public async Task<Reservacion> ProcesarReservacionAsync(Reservacion reservacion)
        {
            // Paso 1: Verificación de Datos de la Reservación.
            reservacion = await ValidarDatosAsync(reservacion);

            // Paso 2: Verificar la existencia del servicio y estilista.
            reservacion = await ValidarServicioEstilistaAsync(reservacion);

            // Paso 3: Cálculo de la duración total.
            reservacion = await CalcularTiempoAtencionAsync(reservacion);

            // Paso 4: Verificar la disponibilidad del estilista.
            reservacion = await ValidarDisponibilidadAsync(reservacion);

            // Paso 5: Persistir la reservación en el repositorio.
            reservacion = await PersistirReservacionAsync(reservacion);


            // Al finalizar, la entidad está validada y lista para ser guardada en el Repositorio.
            return reservacion;
        }
    }
}