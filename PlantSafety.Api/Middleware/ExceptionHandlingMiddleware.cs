// ============================================================================
// File: ExceptionHandlingMiddleware.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-12
// Last Updated: 2026-08-03 (Claude Code performance-optimization-agent) -- Regression audit fix
//   (Handbook §2.19-2.23, §2.30 Canonical Logging Code Examples): this middleware previously
//   logged the raw exception via a message template ("Unhandled exception. Path: {Path}, ...")
//   instead of a single sanitized structured error envelope, had no SchemaVersion field, and did
//   not include TraceId/SpanId/TenantId lineage. Converted to emit a canonical error envelope via
//   IApplicationLogger + ExceptionSanitizer (never the raw stack trace -- only StackTraceHash).
// Description: Global exception handling middleware that maps unhandled exceptions
//              to RFC 7807 ProblemDetails responses without leaking stack traces.
// ============================================================================
//
// Onboarding Notes:
// - This middleware is the last safety net for exceptions not handled by the Result
//   pattern within command/query handlers. The Result pattern (Result<T>) is the
//   preferred mechanism for known business outcomes — exceptions here are truly unexpected.
// - Exception details are NEVER written to the response body to prevent information leakage.
//   The CorrelationId in the response allows operators to correlate the response to the
//   full structured log entry (which does contain the sanitized exception detail).
// - Logs a single structured error envelope (never a message template) via IApplicationLogger,
//   sanitized via ExceptionSanitizer -- StackTraceHash only, never the raw stack trace.
// - See Architecture Specification §4.10 (Global Exception Handling).
//
// Rationale:
// - Provides a consistent RFC 7807 error shape for all unhandled failures, preventing
//   stack traces or framework error pages from reaching API consumers.
//
// Future Extension Seams:
// - Map specific exception types (e.g., ConcurrencyException) to HTTP 409 Conflict.
// - Integrate with Azure Application Insights exception telemetry for automated alerting.
//
// ============================================================================

using System.Text.Json;
using PlantSafety.Api.Models;
using PlantSafety.Application.Abstractions.Context;
using PlantSafety.Application.Abstractions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Logging;
using PlantSafety.Application.Features.EnterpriseFabric.Handlers.Users;

namespace PlantSafety.Api.Middleware;

/// <summary>
/// Middleware that catches unhandled exceptions and converts them to RFC 7807
/// ProblemDetails JSON responses. Exception details are logged but never exposed to callers.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Initializes a new instance of <see cref="ExceptionHandlingMiddleware"/>.
    /// </summary>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Executes the next middleware and catches any unhandled exceptions, converting them
    /// to RFC 7807 ProblemDetails responses with HTTP 500 status.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/>.</param>
    /// <param name="applicationLogger">The canonical structured-envelope logger.</param>
    /// <param name="operationContext">The current operation context carrying correlation lineage.</param>
    public async Task InvokeAsync(HttpContext context, IApplicationLogger applicationLogger, IOperationContext operationContext)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var correlationId = operationContext.CorrelationId != Guid.Empty
                ? operationContext.CorrelationId.ToString()
                : context.TraceIdentifier;

            var sanitized = ExceptionSanitizer.Sanitize(ex);

            applicationLogger.LogError(new OperationLogEnvelope
            {
                OperationName = "Api.UnhandledException",
                HandlerType = context.Request.Path.Value,
                CorrelationId = operationContext.CorrelationId,
                CausationId = operationContext.CausationId,
                RequestId = operationContext.RequestId,
                TraceId = operationContext.TraceId,
                SpanId = operationContext.SpanId,
                TenantId = operationContext.TenantId,
                TimestampUtc = DateTime.UtcNow,
                Success = false,
                ExceptionType = sanitized.ExceptionType,
                Message = sanitized.Message,
                HResult = sanitized.HResult,
                StackTraceHash = sanitized.StackTraceHash,
                Severity = sanitized.Severity == "Warning" ? LogSeverity.Warning : LogSeverity.Error
            });

            await WriteErrorResponseAsync(context, ex, correlationId);
        }
    }

    // Maps the well-known Application-layer exception types to the HTTP status codes their own
    // doc comments already promise (ConcurrencyConflictException -> 409, ApplicationAuthorizationException
    // -> 403, DomainRuleException -> 400) instead of the previous blanket 500. This middleware was
    // the only thing standing between those documented contracts and every unhandled exception
    // actually surfacing as a 500 Internal Server Error, which silently broke ETag/optimistic-
    // concurrency conflict reporting for any handler that lets the exception propagate.
    private static async Task WriteErrorResponseAsync(HttpContext context, Exception ex, string correlationId)
    {
        var problem = ex switch
        {
            ConcurrencyConflictException => ProblemDetailsExtensions.ConflictProblem(ex.Message, correlationId),
            ApplicationAuthorizationException => ProblemDetailsExtensions.ForbiddenProblem(correlationId),
            DomainRuleException => ProblemDetailsExtensions.ValidationProblem([ex.Message], correlationId),
            // Phase 7 §3.4 License Enforcement (2026-08-03): surface the tenant's plan user-cap
            // rejection as a clear 409 Conflict with its own message, instead of a generic 500 --
            // this is a real, enforced business outcome (CreateUserCommandHandler), not an
            // unexpected failure.
            UserLimitExceededException => ProblemDetailsExtensions.ConflictProblem(ex.Message, correlationId),
            _ => ProblemDetailsExtensions.InternalServerErrorProblem(correlationId)
        };

        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problem, JsonOptions);
        await context.Response.WriteAsync(json);
    }
}
