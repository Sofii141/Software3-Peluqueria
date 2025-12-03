using System;

namespace Peluqueria.Application.Exceptions
{
    /// <summary>
    /// Clase base abstracta para todas las excepciones personalizadas del dominio "Peluquería".
    /// </summary>
    /// <remarks>
    /// Cualquier excepción que herede de esta clase obligatoriamente debe proporcionar un <see cref="CodigoError"/>.
    /// Esto permite que el Middleware capture la excepción y genere una respuesta JSON estandarizada.
    /// </remarks>
    public abstract class PeluqueriaException : Exception
    {
        /// <summary>
        /// El código de error estandarizado asociado a la excepción.
        /// </summary>
        public CodigoError CodigoError { get; }

        /// <summary>
        /// Constructor base utilizando el mensaje por defecto del código de error.
        /// </summary>
        protected PeluqueriaException(CodigoError codigoError) : base(codigoError.LlaveMensaje)
        {
            CodigoError = codigoError;
        }

        /// <summary>
        /// Constructor base permitiendo sobrescribir el mensaje descriptivo.
        /// </summary>
        protected PeluqueriaException(CodigoError codigoError, string mensajePersonalizado) : base(mensajePersonalizado)
        {
            CodigoError = codigoError;
        }
    }
}