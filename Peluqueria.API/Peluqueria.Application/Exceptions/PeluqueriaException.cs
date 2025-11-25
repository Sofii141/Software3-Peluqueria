namespace Peluqueria.Application.Exceptions
{
    public abstract class PeluqueriaException : Exception
    {
        public CodigoError CodigoError { get; }

        protected PeluqueriaException(CodigoError codigoError) : base(codigoError.LlaveMensaje)
        {
            CodigoError = codigoError;
        }

        protected PeluqueriaException(CodigoError codigoError, string mensajePersonalizado) : base(mensajePersonalizado)
        {
            CodigoError = codigoError;
        }
    }
}