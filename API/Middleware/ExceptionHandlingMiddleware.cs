using System.Net;
using System.Text.Json;

namespace EkubCircle.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "API request failed: {Path}", context.Request.Path);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                InvalidOperationException => (int)HttpStatusCode.BadRequest,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var message = context.Response.StatusCode == 500
                ? "An unexpected server error occurred."
                : ex.Message;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { message }));
        }
    }
}
