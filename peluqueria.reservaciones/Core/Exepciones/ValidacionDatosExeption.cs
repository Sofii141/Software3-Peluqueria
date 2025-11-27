using System;

namespace peluqueria.reservaciones.Core.Excepciones
{

    public class ValidacionDatosExeption : Exception
    {
        public ValidacionDatosExeption()
        {
        }

        public ValidacionDatosExeption(string message)
            : base(message)
        {
        }

        public ValidacionDatosExeption(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}