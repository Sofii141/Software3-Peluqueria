    using System;

/*
 @uthor: Juan David Moran
    @description: Clase de excepcion personalizada para validaciones referentes al dominio de la aplicacion
    como validaciones de nulos para servicios y estilistas.
 */

namespace peluqueria.reservaciones.Core.Excepciones
    {

        public class ValidacionDominioExcepcion : Exception
        {
            public ValidacionDominioExcepcion()
            {
            }

            public ValidacionDominioExcepcion(string message)
                : base(message)
            {
            }

            public ValidacionDominioExcepcion(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }
    }