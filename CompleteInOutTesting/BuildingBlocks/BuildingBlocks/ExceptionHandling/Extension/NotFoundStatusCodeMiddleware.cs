using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.ExceptionHandling.Extension;

public class NotFoundStatusCodeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<NotFoundStatusCodeMiddleware> _logger;

    public NotFoundStatusCodeMiddleware(
        RequestDelegate next,
        ILogger<NotFoundStatusCodeMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.Response.StatusCode == StatusCodes.Status404NotFound
            && !context.Response.HasStarted)
        {
            _logger.LogWarning(
                "Resource not found. Path: {Path}, TraceId: {TraceId}",
                context.Request.Path,
                context.TraceIdentifier);

            var problemDetails = new ProblemDetails
            {
                Title = "Resource Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"The requested URL {context.Request.Path} was not found.",
                Instance = context.Request.Path
            };

            problemDetails.Extensions["traceId"] =
                context.TraceIdentifier;

            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
