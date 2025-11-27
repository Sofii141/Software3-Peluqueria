    using System;

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

            // Opcional: Útil si quieres incluir la excepción interna que causó el problema.
            public ValidacionDominioExcepcion(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }
    }