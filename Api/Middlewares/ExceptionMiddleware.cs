using Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace Api.Middlewares
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
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = exception switch
            {
                BusinessException => (int)HttpStatusCode.BadRequest,
                NotFoundException => (int)HttpStatusCode.NotFound,
                UnauthorizedActionException => (int)HttpStatusCode.Forbidden,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var message = exception switch
            {
                BusinessException => exception.Message,
                NotFoundException => exception.Message,
                UnauthorizedActionException => exception.Message,
                UnauthorizedAccessException => exception.Message,
                _ => "Ocurrió un error interno en el servidor"
            };

            // Log detallado según el tipo de error
            if (statusCode >= 500)
            {
                _logger.LogError(exception, "Error interno del servidor: {Message}", exception.Message);
            }
            else if (statusCode == 401 || statusCode == 403)
            {
                _logger.LogWarning("Error de autorización: {Message}", exception.Message);
            }
            else
            {
                _logger.LogInformation("Error de negocio: {Message}", exception.Message);
            }

            context.Response.StatusCode = statusCode;

            var response = new
            {
                statusCode = statusCode,
                message = message,
                details = statusCode >= 500 ? null : exception.Message
            };

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
