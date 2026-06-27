// ============================================================================
// File: ProblemDetailsExtensions.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-12
// Last Updated: 2026-06-12
// Description: Extension helpers for building RFC 7807 ProblemDetails responses
//              from domain errors and unhandled exceptions.
// ============================================================================
//
// Onboarding Notes:
// - RFC 7807 ProblemDetails is the standard error format for all non-200 responses.
// - Used by ExceptionHandlingMiddleware and ApiValidationFilter to produce consistent
//   structured error payloads. See Architecture Specification §4.10 (Cross-Cutting Concerns).
// - The CorrelationId is always included in the Extensions dictionary for tracing.
//
// Rationale:
// - Separates ProblemDetails construction logic from middleware and filters,
//   keeping each class focused on a single responsibility.
//
// Future Extension Seams:
// - Add support for problem type URIs referencing the PlantSafety documentation portal.
// - Extend with domain-specific error codes for machine-readable categorisation.
//
// ============================================================================

using Microsoft.AspNetCore.Mvc;

namespace PlantSafety.Api.Models;

/// <summary>
/// Provides factory methods for constructing RFC 7807 <see cref="ProblemDetails"/> responses.
/// </summary>
public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Creates a 400 Bad Request ProblemDetails response for validation failures.
    /// </summary>
    /// <param name="errors">The collection of validation error messages.</param>
    /// <param name="correlationId">The request correlation ID for tracing.</param>
    /// <returns>A <see cref="ProblemDetails"/> configured for HTTP 400.</returns>
    public static ProblemDetails ValidationProblem(IEnumerable<string> errors, string correlationId) =>
        new()
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Failed",
            Detail = "One or more validation errors occurred. See the errors field for details.",
            Extensions =
            {
                ["correlationId"] = correlationId,
                ["errors"] = errors.ToArray()
            }
        };

    /// <summary>
    /// Creates a 401 Unauthorized ProblemDetails response.
    /// </summary>
    /// <param name="correlationId">The request correlation ID for tracing.</param>
    /// <returns>A <see cref="ProblemDetails"/> configured for HTTP 401.</returns>
    public static ProblemDetails UnauthorizedProblem(string correlationId) =>
        new()
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = "The request requires a valid authenticated identity.",
            Extensions = { ["correlationId"] = correlationId }
        };

    /// <summary>
    /// Creates a 403 Forbidden ProblemDetails response.
    /// </summary>
    /// <param name="correlationId">The request correlation ID for tracing.</param>
    /// <returns>A <see cref="ProblemDetails"/> configured for HTTP 403.</returns>
    public static ProblemDetails ForbiddenProblem(string correlationId) =>
        new()
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Detail = "The authenticated identity does not have permission to perform this operation.",
            Extensions = { ["correlationId"] = correlationId }
        };

    /// <summary>
    /// Creates a 404 Not Found ProblemDetails response.
    /// </summary>
    /// <param name="resourceType">The type of resource that was not found.</param>
    /// <param name="correlationId">The request correlation ID for tracing.</param>
    /// <returns>A <see cref="ProblemDetails"/> configured for HTTP 404.</returns>
    public static ProblemDetails NotFoundProblem(string resourceType, string correlationId) =>
        new()
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Not Found",
            Detail = $"The requested {resourceType} could not be found.",
            Extensions = { ["correlationId"] = correlationId }
        };

    /// <summary>
    /// Creates a 409 Conflict ProblemDetails response for domain invariant violations.
    /// </summary>
    /// <param name="detail">A human-readable explanation of the conflict.</param>
    /// <param name="correlationId">The request correlation ID for tracing.</param>
    /// <returns>A <see cref="ProblemDetails"/> configured for HTTP 409.</returns>
    public static ProblemDetails ConflictProblem(string detail, string correlationId) =>
        new()
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Conflict",
            Detail = detail,
            Extensions = { ["correlationId"] = correlationId }
        };

    /// <summary>
    /// Creates a 500 Internal Server Error ProblemDetails response for unhandled exceptions.
    /// The exception detail is never exposed to callers to prevent information leakage.
    /// </summary>
    /// <param name="correlationId">The request correlation ID for tracing.</param>
    /// <returns>A <see cref="ProblemDetails"/> configured for HTTP 500.</returns>
    public static ProblemDetails InternalServerErrorProblem(string correlationId) =>
        new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred. Please contact support with the provided correlation ID.",
            Extensions = { ["correlationId"] = correlationId }
        };
}
