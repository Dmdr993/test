using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Citas.Api.Infrastructure;

public sealed class ValidationException(string message) : Exception(message);
public sealed class ScheduleConflictException(string message) : Exception(message);

public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            if (exception is SqlException { Number: 51001 })
                exception = new ScheduleConflictException(exception.Message);

            var (status, title) = exception switch
            {
                ValidationException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
                ScheduleConflictException => (StatusCodes.Status409Conflict, "Horario no disponible"),
                _ => (StatusCodes.Status500InternalServerError, "Error interno")
            };

            if (status == 500) logger.LogError(exception, "Error no controlado");
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";
            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == 500 ? "Ocurrió un error inesperado." : exception.Message
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}

