using System.Net;
using System.Text.Json;

namespace HW2.Middlewares;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions ResponseJsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Resource not found"),
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized"),
            InvalidOperationException => ((int)HttpStatusCode.Conflict, "Conflict"),
            ArgumentException => ((int)HttpStatusCode.BadRequest, "Bad request"),
            OperationCanceledException => ((int)HttpStatusCode.RequestTimeout, "Request canceled"),
            _ => ((int)HttpStatusCode.InternalServerError, "Internal server error")
        };

        logger.Log(
            statusCode >= (int)HttpStatusCode.InternalServerError ? LogLevel.Error : LogLevel.Warning,
            exception,
            "Unhandled exception while processing {RequestMethod} {RequestPath}. TraceId: {TraceId}",
            context.Request.Method,
            context.Request.Path.Value,
            context.TraceIdentifier);

        if (context.Response.HasStarted)
        {
            logger.LogWarning(
                "Response already started, cannot write error body for {RequestPath}",
                context.Request.Path.Value);
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            type = $"https://httpstatuses.com/{statusCode}",
            title,
            status = statusCode,
            traceId = context.TraceIdentifier
        }, ResponseJsonOptions));
    }
}