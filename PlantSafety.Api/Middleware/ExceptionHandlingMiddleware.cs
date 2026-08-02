// ============================================================================
// File: ExceptionHandlingMiddleware.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-12
// Last Updated: 2026-06-12
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
//   full structured log entry (which does contain the exception detail).
// - Logging the exception at Error level occurs here. Infrastructure's Serilog pipeline
//   enriches the log entry with CorrelationId, TenantId, and RequestPath automatically.
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
using PlantSafety.Application.Common.Errors;

namespace PlantSafety.Api.Middleware;

/// <summary>
/// Middleware that catches unhandled exceptions and converts them to RFC 7807
/// ProblemDetails JSON responses. Exception details are logged but never exposed to callers.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Initializes a new instance of <see cref="ExceptionHandlingMiddleware"/>.
    /// </summary>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <param name="logger">The logger instance for recording unhandled exceptions.</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Executes the next middleware and catches any unhandled exceptions, converting them
    /// to RFC 7807 ProblemDetails responses with HTTP 500 status.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/>.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.Items[CorrelationIdMiddleware.ItemsKey]?.ToString()
                                ?? context.TraceIdentifier;

            _logger.LogError(
                ex,
                "Unhandled exception. Path: {Path}, CorrelationId: {CorrelationId}",
                context.Request.Path,
                correlationId);

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
            _ => ProblemDetailsExtensions.InternalServerErrorProblem(correlationId)
        };

        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problem, JsonOptions);
        await context.Response.WriteAsync(json);
    }
}
