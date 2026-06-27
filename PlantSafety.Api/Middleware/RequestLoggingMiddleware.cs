// ============================================================================
// File: RequestLoggingMiddleware.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-12
// Last Updated: 2026-06-12
// Description: Middleware that logs structured HTTP request and response metadata
//              including method, path, status code, and duration for observability.
// ============================================================================
//
// Onboarding Notes:
// - This middleware emits structured log entries enriched with CorrelationId, Method,
//   Path, StatusCode, and Duration. The Serilog pipeline in Infrastructure adds
//   TenantId and UserId via LogContext enrichers — this middleware does NOT add those.
// - The request BODY is never logged to prevent PII/credential leakage.
// - Health check endpoints (/health, /health/ready, /health/live) are intentionally
//   excluded from logging to avoid flooding the structured log store.
// - Uses ILogger<T> (not Serilog directly) per Clean Architecture — Serilog is an
//   Infrastructure concern wired as the Microsoft.Extensions.Logging provider.
//   See Architecture Specification §4.1 (Presentation Layer Rules).
//
// Rationale:
// - Every HTTP request must generate a structured log entry for operational dashboards
//   and SLA monitoring (P99 latency tracked in Grafana via Prometheus metrics).
//
// Future Extension Seams:
// - Emit Prometheus histogram metrics for request duration per endpoint.
// - Add response body size to the structured log entry.
//
// ============================================================================

using System.Diagnostics;

namespace PlantSafety.Api.Middleware;

/// <summary>
/// Middleware that emits a structured log entry for each HTTP request, capturing
/// method, path, response status, and elapsed duration.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    private static readonly string[] ExcludedPaths =
    [
        "/health",
        "/health/ready",
        "/health/live"
    ];

    /// <summary>
    /// Initializes a new instance of <see cref="RequestLoggingMiddleware"/>.
    /// </summary>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <param name="logger">The logger instance for emitting request telemetry.</param>
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Measures request duration and emits a structured log entry after the response
    /// has been written. Health check paths are excluded.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/>.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (IsExcluded(path))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var correlationId = context.Items[CorrelationIdMiddleware.ItemsKey]?.ToString()
                                ?? context.TraceIdentifier;

            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms. CorrelationId: {CorrelationId}",
                context.Request.Method,
                path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId);
        }
    }

    private static bool IsExcluded(string path) =>
        ExcludedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
}
