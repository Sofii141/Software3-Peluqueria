using System;
using peluqueria.reservaciones.Aplicacion.DTO.Comunicacion;

/*
 @author: Juan Dabid Moran
    @description: DTO para devolver informacion referente a los reportes.
 */

namespace peluqueria.reservaciones.Aplicacion.DTO.Comunicacion
{
    public class InfoReportesRespuestaDTO
    {
        public int NumeroReservacionesFinalizadas { get; set; }
        public int NumeroReservacionesNoShow { get; set; }
        public int NumeroReservacionesCanceladas { get; set; }
        public List<ServicioRespuestaDTO> ServiciosRealizados { get; set; } = new List<ServicioRespuestaDTO>();

    }
}