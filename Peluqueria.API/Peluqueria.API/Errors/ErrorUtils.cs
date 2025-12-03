namespace Peluqueria.API.Errors
{
    /// <summary>
    /// Clase de utilidad estática para facilitar la creación de objetos de respuesta de error estandarizados.
    /// </summary>
    public static class ErrorUtils
    {
        /// <summary>
        /// Crea una nueva instancia de <see cref="ErrorResponse"/> con los detalles del error especificados.
        /// </summary>
        /// <param name="codigo">Código interno del error de negocio (ej. 'G-ERROR-001').</param>
        /// <param name="mensaje">Mensaje descriptivo del error.</param>
        /// <param name="httpStatus">Código de estado HTTP asociado al error.</param>
        /// <param name="url">URL de la solicitud que generó el error.</param>
        /// <param name="metodo">Método HTTP de la solicitud (GET, POST, etc.).</param>
        /// <returns>Un objeto <see cref="ErrorResponse"/> configurado.</returns>
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