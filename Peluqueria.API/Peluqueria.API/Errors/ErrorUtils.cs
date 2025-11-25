namespace Peluqueria.API.Errors
{
    public static class ErrorUtils
    {
        public static ErrorResponse CrearError(string codigo, string mensaje, int httpStatus, string url, string metodo)
        {
            return new ErrorResponse
            {
                CodigoError = codigo,
                Mensaje = mensaje,
                CodigoHttp = httpStatus,
                Url = url,
                Metodo = metodo
            };
        }
    }
}