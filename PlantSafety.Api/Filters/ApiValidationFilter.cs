// ============================================================================
// File: ApiValidationFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-12
// Last Updated: 2026-06-12
// Description: Action filter that intercepts invalid ModelState and returns a
//              consistent RFC 7807 ProblemDetails 400 response before the controller executes.
// ============================================================================
//
// Onboarding Notes:
// - Registered globally via MvcOptions.Filters in ServiceCollectionExtensions.cs so it
//   applies to every controller action without per-controller decoration.
// - Works in conjunction with FluentValidation validators in the Application layer:
//   DataAnnotation-based model binding errors are caught here; business rule validation
//   errors are handled by ValidationBehavior in the MediatR pipeline and mapped to
//   Result<T>.Failure in the handler — which controllers then map to 400 responses.
// - The CorrelationId is retrieved from HttpContext.Items to include in the error response.
// - See Architecture Specification §4.10 and §10.4.2 Rule 5 (Filters).
//
// Rationale:
// - Centralising validation error response formatting prevents each controller from
//   needing its own ModelState.IsValid check and error mapping code.
//
// Future Extension Seams:
// - Add field-level error details (property name + error message pairs) to the Extensions dict.
// - Integrate with OpenTelemetry to count validation failure rates per endpoint.
//
// ============================================================================

using Microsoft.AspNetCore.Mvc.Filters;
using PlantSafety.Api.Middleware;
using PlantSafety.Api.Models;

namespace PlantSafety.Api.Filters;

/// <summary>
/// Global action filter that short-circuits controller execution when ModelState is invalid,
/// returning an RFC 7807 ProblemDetails 400 Bad Request response.
/// </summary>
public sealed class ApiValidationFilter : IActionFilter
{
    /// <summary>
    /// Checks ModelState validity before the action executes. If invalid, writes a
    /// ProblemDetails 400 response and prevents further pipeline execution.
    /// </summary>
    /// <param name="context">The action-executing context containing ModelState and HttpContext.</param>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
        {
            return;
        }

        var correlationId = context.HttpContext.Items[CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? context.HttpContext.TraceIdentifier;

        var errors = context.ModelState
            .Where(kv => kv.Value?.Errors.Count > 0)
            .SelectMany(kv => kv.Value!.Errors.Select(e =>
                string.IsNullOrWhiteSpace(e.ErrorMessage) ? "A validation error occurred." : e.ErrorMessage))
            .ToList();

        var problem = ProblemDetailsExtensions.ValidationProblem(errors, correlationId);
        context.Result = new ObjectResult(problem)
        {
            StatusCode = StatusCodes.Status400BadRequest,
            ContentTypes = { "application/problem+json" }
        };
    }

    /// <inheritdoc />
    public void OnActionExecuted(ActionExecutedContext context) { }
}
