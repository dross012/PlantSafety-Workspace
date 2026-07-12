// ============================================================================
// File: BaseApiController.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Abstract base controller providing shared helpers for correlation ID
//              extraction, tenant context access, and standardised ApiResponse factories.
// ============================================================================
//
// Onboarding Notes:
// - All module V1 and V2 controllers should inherit from BaseApiController instead
//   of ControllerBase directly. This eliminates the repeated
//   HttpContext.Items[CorrelationIdMiddleware.ItemsKey] pattern found in all
//   generated controllers and ensures uniform response shaping.
// - TenantId and SiteId properties delegate to TenantContext, which is populated
//   by TenantResolutionMiddleware. They are safe to access in any controller action
//   that sits behind authentication middleware.
// - The Ok<T>, BadRequest<T>, NotFound<T>, and Created<T> overloads wrap results in
//   ApiResponse<T> automatically, so controller actions stay concise.
//
// Rationale:
// - Removing boilerplate from 121+ controllers reduces maintenance surface and
//   ensures correlation IDs and tenant context are accessed consistently.
//
// Future Extension Seams:
// - Add a CallerUserId property resolved from JWT sub claim for audit trail use.
// - Add a RequestedAt property (UtcNow at action entry) for latency measurement.
//
// ============================================================================

using PlantSafety.Api.CrossDomain;
using PlantSafety.Api.Constants;

namespace PlantSafety.Api.Controllers;

/// <summary>
/// Abstract base controller providing shared helpers for all PlantSafety.Api controllers.
/// Inherit this instead of <see cref="ControllerBase"/> directly.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Gets the correlation ID for the current request, propagated by
    /// <see cref="Middleware.CorrelationIdMiddleware"/>.
    /// </summary>
    protected string CorrelationId =>
        HttpContext.Items[Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString() ?? string.Empty;

    /// <summary>
    /// Gets the resolved tenant identifier from <see cref="TenantContext"/>.
    /// Populated by <see cref="Middleware.TenantResolutionMiddleware"/> after authentication.
    /// </summary>
    protected Guid TenantId =>
        HttpContext.Items[TenantContextAccessor.TenantIdKey] is Guid g ? g : Guid.Empty;

    /// <summary>
    /// Gets the resolved site identifier from <see cref="TenantContext"/>, if present.
    /// </summary>
    protected Guid? SiteId =>
        HttpContext.Items[TenantContextAccessor.SiteIdKey] as Guid?;

    // ── ApiResponse factory overloads ─────────────────────────────────────────

    /// <summary>Returns a 200 OK response wrapped in the canonical <see cref="ApiResponse{T}"/>.</summary>
    /// <typeparam name="T">Response payload type.</typeparam>
    /// <param name="data">The response payload.</param>
    protected IActionResult ApiOk<T>(T data) =>
        Ok(ApiResponse<T>.Ok(data, CorrelationId));

    /// <summary>Returns a 201 Created response wrapped in <see cref="ApiResponse{T}"/>.</summary>
    /// <typeparam name="T">Response payload type.</typeparam>
    /// <param name="actionName">The action name for the Location header.</param>
    /// <param name="routeValues">Route values for the Location header.</param>
    /// <param name="data">The created resource payload.</param>
    protected IActionResult ApiCreated<T>(string actionName, object routeValues, T data) =>
        CreatedAtAction(actionName, routeValues, ApiResponse<T>.Ok(data, CorrelationId));

    /// <summary>Returns a 400 Bad Request response wrapped in <see cref="ApiResponse{T}"/>.</summary>
    /// <typeparam name="T">Expected response payload type (default for error responses).</typeparam>
    /// <param name="message">Error message.</param>
    protected IActionResult ApiBadRequest<T>(string message) =>
        BadRequest(ApiResponse<T>.Fail(message, CorrelationId));

    /// <summary>Returns a 404 Not Found response wrapped in <see cref="ApiResponse{T}"/>.</summary>
    /// <typeparam name="T">Expected response payload type.</typeparam>
    /// <param name="message">Not-found detail message.</param>
    protected IActionResult ApiNotFound<T>(string message) =>
        NotFound(ApiResponse<T>.Fail(message, CorrelationId));

    /// <summary>Returns a 409 Conflict response wrapped in <see cref="ApiResponse{T}"/>.</summary>
    /// <typeparam name="T">Expected response payload type.</typeparam>
    /// <param name="message">Conflict detail message.</param>
    protected IActionResult ApiConflict<T>(string message) =>
        Conflict(ApiResponse<T>.Fail(message, CorrelationId));

    /// <summary>
    /// Maps a <see cref="PlantSafety.SharedKernel.Primitives.Results.Result{T}"/> to the appropriate
    /// <see cref="IActionResult"/> based on success or failure.
    /// </summary>
    /// <typeparam name="TResult">Application-layer result value type.</typeparam>
    /// <typeparam name="TResponse">Presentation-layer response DTO type.</typeparam>
    /// <param name="result">The Application-layer result.</param>
    /// <param name="mapper">Projects the result value to the response DTO.</param>
    protected IActionResult FromResult<TResult, TResponse>(
        PlantSafety.SharedKernel.Primitives.Results.Result<TResult> result,
        Func<TResult, TResponse> mapper)
    {
        if (result.IsFailure)
            return BadRequest(ApiResponse<TResponse>.Fail(result.Error.Message, CorrelationId));
        return Ok(ApiResponse<TResponse>.Ok(mapper(result.Value), CorrelationId));
    }
}
