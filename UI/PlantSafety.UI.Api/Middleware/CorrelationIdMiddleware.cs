// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Middleware
//  File:           CorrelationIdMiddleware.cs
//  Created:        2026-08-09
//  Description:    Phase 2.4 deliverable: telemetry pipeline integration.
//                  The very first middleware in the pipeline (Program.cs).
//                  Reads the X-Correlation-Id header PlantSafetyApiClient
//                  already sends on every request, or generates one if
//                  absent (a direct browser/curl call, not through the
//                  approved client), sets it on the request-scoped
//                  CorrelationContext, echoes it back on the response, and
//                  wraps the rest of the pipeline in an ILogger scope so
//                  every log line emitted while handling this request --
//                  including ones from GlobalErrorHandler -- carries the
//                  same correlation id automatically.
// ============================================================================
using Shared.Telemetry.Abstractions.Correlation;

namespace PlantSafety.UI.Api.Middleware;

/// <summary>Establishes the request's correlation id and makes it ambient for the rest of the pipeline.</summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    /// <summary>Invokes the middleware.</summary>
    public async Task InvokeAsync(HttpContext context, CorrelationContext correlationContext, ILoggerFactory loggerFactory)
    {
        string correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeader, out var headerValue) && !string.IsNullOrWhiteSpace(headerValue)
            ? headerValue.ToString()
            : Guid.NewGuid().ToString();

        correlationContext.Set(correlationId);
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        ILogger logger = loggerFactory.CreateLogger("PlantSafety.UI.Api.CorrelationIdMiddleware");
        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await next(context);
        }
    }
}
