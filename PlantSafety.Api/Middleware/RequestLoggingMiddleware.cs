// ============================================================================
// File: RequestLoggingMiddleware.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-12
// Last Updated: 2026-08-03 (Claude Code performance-optimization-agent) -- Regression audit fix
//   (Handbook §2.19 Success Logging Architectural Rule, §2.22 Canonical Log Envelope Rule,
//   §2.31.1 Canonical API Controller Logging Example): this middleware previously emitted a
//   single message-template log ("HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms...")
//   instead of the mandated structured success envelope, had no SchemaVersion field, and did not
//   propagate TenantId/ActorId/TraceId/SpanId. Converted to emit a canonical success envelope via
//   IApplicationLogger. Also now feeds the new PlatformHealthDashboard request-metrics capture
//   pipeline (Handbook Phase 6 §5.3) via IRequestMetricsRecorder -- fire-and-forget, never blocks
//   the response.
// Description: Middleware that logs structured HTTP request and response metadata
//              including method, path, status code, and duration for observability.
// ============================================================================
//
// Onboarding Notes:
// - This middleware emits one structured success envelope object per request (Handbook §2.22 --
//   never a message template). The Serilog pipeline in Infrastructure adds TenantId and UserId
//   via LogContext enrichers in addition to the envelope fields set here.
// - The request BODY is never logged to prevent PII/credential leakage.
// - Health check endpoints (/health, /health/ready, /health/live) are intentionally
//   excluded from logging to avoid flooding the structured log store.
// - Uses IApplicationLogger (Application-layer abstraction), not ILogger directly, so every
//   HTTP request emits the same canonical envelope shape as Application-layer handlers.
//
// Rationale:
// - Every HTTP request must generate a structured log entry for operational dashboards
//   and SLA monitoring (P50/P95/P99 latency surfaced by PlatformHealthDashboard).
//
// Future Extension Seams:
// - Add response body size to the structured log entry.
//
// ============================================================================

using System.Diagnostics;
using PlantSafety.Application.Abstractions.Context;
using PlantSafety.Application.Abstractions.Logging;
using PlantSafety.Application.Abstractions.Observability;

namespace PlantSafety.Api.Middleware;

/// <summary>
/// Middleware that emits a canonical structured success envelope for each HTTP request,
/// capturing method, path, response status, and elapsed duration (Handbook §2.31.1).
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

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
    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Measures request duration and emits a structured success envelope after the response
    /// has been written. Health check paths are excluded.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/>.</param>
    /// <param name="applicationLogger">The canonical structured-envelope logger.</param>
    /// <param name="operationContext">The current operation context carrying correlation lineage.</param>
    /// <param name="requestMetricsRecorder">Fire-and-forget request-metrics capture for the PlatformHealthDashboard.</param>
    public async Task InvokeAsync(
        HttpContext context,
        IApplicationLogger applicationLogger,
        IOperationContext operationContext,
        IRequestMetricsRecorder requestMetricsRecorder)
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

            var method = context.Request.Method;
            var statusCode = context.Response.StatusCode;
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            applicationLogger.LogSuccess(new OperationLogEnvelope
            {
                OperationName = $"HTTP.{method}",
                HandlerType = path,
                CorrelationId = operationContext.CorrelationId,
                CausationId = operationContext.CausationId,
                RequestId = operationContext.RequestId,
                TraceId = operationContext.TraceId,
                SpanId = operationContext.SpanId,
                TenantId = operationContext.TenantId,
                TimestampUtc = DateTime.UtcNow,
                ElapsedMs = elapsedMs,
                Success = statusCode < 500,
                Severity = statusCode >= 500 ? LogSeverity.Error : LogSeverity.Information
            });

            // Fire-and-forget, non-blocking: never let metrics capture slow down or fault the
            // response. See ChannelRequestMetricsRecorder remarks for the bounded-channel design.
            requestMetricsRecorder.Record(method, path, statusCode, elapsedMs, operationContext.CorrelationId, operationContext.TenantId);
        }
    }

    private static bool IsExcluded(string path) =>
        ExcludedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
}
