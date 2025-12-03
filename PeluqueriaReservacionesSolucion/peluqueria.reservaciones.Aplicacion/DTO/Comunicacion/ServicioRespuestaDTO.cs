using System;

/*
 @author: Juan Dabid Moran
    @description: DTO para devolver informacion referente a los reportes.
 */

namespace peluqueria.reservaciones.Aplicacion.DTO.Comunicacion
{
    public class ServicioRespuestaDTO
    {
        public string ServicioNombre { get; set; }
        public int TotalReservaciones { get; set; }
    }
}