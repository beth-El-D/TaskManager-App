using TaskManager.API.Exceptions;
using System.Net;
using System.Text.Json;

namespace TaskManager.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();

        switch (exception)
        {
            case UserAlreadyExistsException:
                response.Message = exception.Message;
                response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Type = "USER_ALREADY_EXISTS";
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                break;

            case InvalidCredentialsException:
                response.Message = exception.Message;
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Type = "INVALID_CREDENTIALS";
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;

            case ValidationException:
                response.Message = exception.Message;
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Type = "VALIDATION_ERROR";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case UnauthorizedException:
                response.Message = exception.Message;
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Type = "UNAUTHORIZED";
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;

            default:
                response.Message = "An unexpected error occurred. Please try again later.";
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Type = "INTERNAL_ERROR";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}