using System.Net;
using System.Text.Json;
using Peluqueria.API.Errors;
using Peluqueria.Application.Exceptions;

namespace Peluqueria.API.Middleware
{
    /// <summary>
    /// Middleware global encargado de interceptar y manejar las excepciones no controladas
    /// ocurridas durante el ciclo de vida de una solicitud HTTP.
    /// </summary>
    /// <remarks>
    /// Este componente implementa el manejo centralizado de errores, asegurando que todas las excepciones,
    /// ya sean de dominio o del sistema, se transformen en una respuesta JSON estandarizada (<see cref="ErrorResponse"/>)
    /// con el código de estado HTTP adecuado.
    /// </remarks>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        /// <summary>
        /// Inicializa una nueva instancia del middleware.
        /// </summary>
        /// <param name="next">Delegado que representa el siguiente componente en el pipeline de solicitud.</param>
        /// <param name="logger">Servicio de logging para registrar los detalles de las excepciones capturadas.</param>
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invoca la ejecución del middleware. Envuelve la ejecución del resto del pipeline en un bloque try-catch.
        /// </summary>
        /// <param name="context">Contexto de la solicitud HTTP actual.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error capturado por Middleware: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Procesa la excepción capturada, determina el código de estado HTTP correspondiente
        /// y escribe la respuesta de error estandarizada en el flujo de salida.
        /// </summary>
        /// <param name="context">Contexto HTTP.</param>
        /// <param name="exception">La excepción capturada.</param>
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            int statusCode;
            string codigoError;
            string mensaje;

            // Mapeo de excepciones de dominio a códigos HTTP
            switch (exception)
            {
                case EntidadNoExisteException ex:
                    statusCode = (int)HttpStatusCode.NotFound; // 404
                    codigoError = ex.CodigoError.Codigo;
                    mensaje = ex.Message;
                    break;

                case EntidadYaExisteException ex:
                    statusCode = (int)HttpStatusCode.Conflict; // 409
                    codigoError = ex.CodigoError.Codigo;
                    mensaje = ex.Message;
                    break;

                case ReglaNegocioException ex:
                    statusCode = (int)HttpStatusCode.BadRequest; // 400
                    codigoError = ex.CodigoError.Codigo;
                    mensaje = ex.Message;
                    break;

                default:
                    // Manejo de errores no controlados (Errores de sistema o infraestructura)
                    statusCode = (int)HttpStatusCode.InternalServerError; // 500
                    codigoError = CodigoError.ERROR_GENERICO.Codigo;
                    // Se utiliza un mensaje genérico por seguridad para no exponer detalles internos
                    mensaje = "Error interno del servidor.";
                    break;
            }

            context.Response.StatusCode = statusCode;

            var errorResponse = ErrorUtils.CrearError(
                codigoError,
                mensaje,
                statusCode,
                context.Request.Path,
                context.Request.Method
            );

            // Serialización de la respuesta a formato JSON (CamelCase estándar)
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(errorResponse, options);

            return context.Response.WriteAsync(json);
        }
    }
}