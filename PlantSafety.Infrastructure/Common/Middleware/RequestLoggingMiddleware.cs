// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common
//  Layer:          Infrastructure
//  File:           RequestLoggingMiddleware.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    ASP.NET Core middleware for structured canonical request/response logging.
//                  Additive-only evolution.
// ============================================================================
using Microsoft.AspNetCore.Http;

namespace PlantSafety.Infrastructure.Common.Middleware;

/// <summary>
/// ASP.NET Core middleware that emits a structured canonical log entry for every
/// HTTP request/response pair including CorrelationId, method, path, and duration.
/// </summary>
public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    /// <inheritdoc cref="RequestDelegate"/>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Items.TryGetValue("CorrelationId", out var cid) ? cid?.ToString() ?? "unknown" : "unknown";
        var start = DateTimeOffset.UtcNow;

        try
        {
            await next(context);
        }
        finally
        {
            var elapsed = (DateTimeOffset.UtcNow - start).TotalMilliseconds;
            logger.LogInformation(
                "HTTP {Method} {Path} => {StatusCode} in {ElapsedMs}ms | CorrelationId={CorrelationId}",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                elapsed,
                correlationId);
        }
    }
}
