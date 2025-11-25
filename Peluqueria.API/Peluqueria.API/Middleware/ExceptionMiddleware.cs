using System.Net;
using System.Text.Json;
using Peluqueria.API.Errors;
using Peluqueria.Application.Exceptions;

namespace Peluqueria.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

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

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            int statusCode;
            string codigoError;
            string mensaje;

            // Evaluamos el tipo de excepción
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

                // Si es una excepción que no controlamos (Bug, NullPointer, DB off)
                default:
                    statusCode = (int)HttpStatusCode.InternalServerError; // 500
                    codigoError = CodigoError.ERROR_GENERICO.Codigo;
                    mensaje = "Error interno del servidor."; // No mostrar ex.Message en prod por seguridad
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

            // Usamos System.Text.Json con nomenclatura camelCase (estándar JSON)
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(errorResponse, options);

            return context.Response.WriteAsync(json);
        }
    }
}