// ============================================================================
// File: ValidationProblemDetailsFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Provides the InvalidModelStateResponseFactory delegate registered
//              in ServiceCollectionExtensions to shape ASP.NET Core model-binding
//              validation failures into RFC 7807 ValidationProblemDetails with
//              per-field error arrays and a correlationId extension.
// ============================================================================
//
// Onboarding Notes:
// - Register by calling ValidationProblemDetailsFilter.Configure(options) inside
//   services.Configure<ApiBehaviorOptions>(...) in ServiceCollectionExtensions.
// - This factory fires when [ApiController] automatic model validation detects
//   invalid ModelState BEFORE the action executes. It does NOT fire for
//   FluentValidation errors returned as Result.IsFailure from the MediatR pipeline.
//   Those are handled in BaseApiController.ApiBadRequest<T>() or FromResult<T,TResponse>().
// - The response shape:
//     {
//       "type": "https://tools.ietf.org/html/rfc7807#validation",
//       "title": "One or more validation errors occurred.",
//       "status": 422,
//       "instance": "/api/v1/compliance/audit-findings",
//       "correlationId": "...",
//       "errors": { "TenantId": ["The TenantId field is required."], ... }
//     }
// - Status 422 is used instead of 400 to distinguish validation errors (user
//   input) from bad request errors (malformed JSON, wrong Content-Type).
//
// Rationale:
// - Without this factory, [ApiController] produces a 400 response with only a
//   generic error string. RFC 7807 field-level errors are essential for front-end
//   form validation and for API consumers debugging integration issues.
//
// Future Extension Seams:
// - Add error code mapping: map FluentValidation ErrorCode values to machine-readable
//   codes in the errors dictionary once the Application layer uses error codes.
//
// ============================================================================

using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace PlantSafety.Api.Filters;

/// <summary>
/// Provides the <see cref="ApiBehaviorOptions.InvalidModelStateResponseFactory"/> delegate
/// that shapes model-binding validation failures into RFC 7807
/// <see cref="ValidationProblemDetails"/> responses with per-field error arrays.
/// </summary>
public static class ValidationProblemDetailsFilter
{
    /// <summary>
    /// Configures <see cref="ApiBehaviorOptions"/> to use the RFC 7807
    /// <see cref="ValidationProblemDetails"/> factory for invalid model state.
    /// Call inside <c>services.Configure&lt;ApiBehaviorOptions&gt;(ValidationProblemDetailsFilter.Configure)</c>.
    /// </summary>
    /// <param name="options">The API behavior options to configure.</param>
    public static void Configure(ApiBehaviorOptions options)
    {
        options.InvalidModelStateResponseFactory = Factory;
    }

    /// <summary>
    /// Builds a 422 <see cref="ValidationProblemDetails"/> response from the current
    /// <see cref="ActionContext.ModelState"/>.
    /// </summary>
    /// <param name="context">The action context containing the invalid model state.</param>
    /// <returns>An <see cref="UnprocessableEntityObjectResult"/> with RFC 7807 body.</returns>
    public static IActionResult Factory(ActionContext context)
    {
        var correlationId = context.HttpContext.Items[Middleware.CorrelationIdMiddleware.ItemsKey]
            ?.ToString() ?? string.Empty;

        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Type     = "https://tools.ietf.org/html/rfc7807#validation",
            Title    = "One or more validation errors occurred.",
            Status   = StatusCodes.Status422UnprocessableEntity,
            Instance = context.HttpContext.Request.Path
        };

        problemDetails.Extensions["correlationId"] = correlationId;
        problemDetails.Extensions["traceId"]       =
            System.Diagnostics.Activity.Current?.Id
            ?? context.HttpContext.TraceIdentifier;

        return new UnprocessableEntityObjectResult(problemDetails)
        {
            ContentTypes = { "application/problem+json" }
        };
    }
}
