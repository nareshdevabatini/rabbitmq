using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.ExceptionHandling.Handler
{
    public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger): IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError($"Error Message: {exception.Message} , time: {DateTime.UtcNow}");
            var problemDetails = new ProblemDetails
            {
                Title= exception.Message,
                Detail = exception.StackTrace,
                Instance = httpContext.Request.Path,
            };
            problemDetails.Extensions.Add("traceId", httpContext.TraceIdentifier);
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
    }
}
