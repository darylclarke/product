using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProductsApi.Exceptions;
using ProductsApi.Services;
using System.Text.Json;

namespace ProductsApi.Middleware;
public class GlobalExceptionHandler(IAppLogger<GlobalExceptionHandler> logger, IWebHostEnvironment env)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails();
        problemDetails.Instance = httpContext.Request.Path;
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        
        if (exception is BaseException e)
        {
            httpContext.Response.StatusCode = (int)e.StatusCode;
            problemDetails.Title = e.Message;
            
            logger.LogWarning(
                "Request {Method} {Path} failed with {StatusCode}: {Message}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                (int)e.StatusCode,
                e.Message);
        }

        else if (exception is JsonException || 
                (exception is BadHttpRequestException badHttpRequestException && 
                (badHttpRequestException.InnerException is JsonException ||
                 badHttpRequestException.Message.Contains("JSON"))))
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            problemDetails.Title = "Invalid JSON in request body";
            
            var details = exception.InnerException?.Message ?? exception.Message;
            problemDetails.Detail = details;
            
            logger.LogWarning(
                "Request {Method} {Path} failed with invalid JSON: {Message}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                details);
        }
        else
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            problemDetails.Title = "An unexpected error occurred";
            
            if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                logger.LogError(exception,
                    "Unhandled exception for request {Method} {Path} from IP {IPAddress}: {Message}",
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    forwardedFor,
                    exception.Message);
            }
            else
            {
                logger.LogError(exception,
                    "Unhandled exception for request {Method} {Path}: {Message}",
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    exception.Message);
            }
            
            if (env.IsDevelopment())
            {
                problemDetails.Detail = exception.ToString();
            }
        }
        
        problemDetails.Status = httpContext.Response.StatusCode;
        problemDetails.Type = $"https://httpstatuses.com/{problemDetails.Status}";
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken).ConfigureAwait(false);
        return true;
    }
}
