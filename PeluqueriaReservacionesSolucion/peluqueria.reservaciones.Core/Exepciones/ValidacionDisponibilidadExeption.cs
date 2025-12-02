using System;

/*
 @uthor: Juan David Moran
    @description: Clase de excepcion personalizada para validaciones referentes a la disponibilidad
    de horarios para las reservaciones.
 */

namespace peluqueria.reservaciones.Core.Excepciones
{
    public class ValidacionDisponibilidadExeption : Exception
    {
        public ValidacionDisponibilidadExeption()
        {
        }
        public ValidacionDisponibilidadExeption(string message)
            : base(message)
        {
        }
        public ValidacionDisponibilidadExeption(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}