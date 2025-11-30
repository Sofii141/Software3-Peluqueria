using System;

/*
 @uthor: Juan David Moran
    @description: Clase de excepcion personalizada para validaciones referentes a la integridad de los datos
    como formatos, rangos y restricciones.
 */

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