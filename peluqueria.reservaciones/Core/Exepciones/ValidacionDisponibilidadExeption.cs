using System;

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