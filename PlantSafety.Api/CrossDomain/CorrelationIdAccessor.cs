// ============================================================================
// File: CorrelationIdAccessor.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Presentation-layer accessor for the request correlation ID stored
//              in HttpContext.Items by CorrelationIdMiddleware. Used in non-controller
//              code (filters, output cache policies, audit log filters) that cannot
//              inherit from BaseApiController.
// ============================================================================
//
// Onboarding Notes:
// - CorrelationIdMiddleware writes the correlation ID into HttpContext.Items under
//   the key CorrelationIdMiddleware.ItemsKey. This accessor provides a typed
//   interface so code outside controllers can retrieve it without referencing the
//   middleware class directly.
// - Returns an empty string when no correlation ID is present (e.g. before
//   CorrelationIdMiddleware has run, which should not happen in normal middleware
//   ordering, but may happen in test scenarios).
// - Register as scoped in DI so each request gets a fresh accessor instance bound
//   to that request's HttpContext.
//
// Rationale:
// - Filters such as AuditLogFilter and IdempotencyFilter need the correlation ID
//   to stamp log entries and idempotency cache keys. This accessor lets them get it
//   without knowing the HttpContext.Items key string.
//
// Future Extension Seams:
// - Expose IsGenerated: bool to indicate whether the ID was client-supplied or
//   generated server-side (useful for telemetry attribution).
//
// ============================================================================

namespace PlantSafety.Api.CrossDomain;

/// <summary>
/// Presentation-layer accessor for the X-Correlation-ID stored in
/// <see cref="HttpContext.Items"/> by <see cref="Middleware.CorrelationIdMiddleware"/>.
/// Inject this into filters and policies that cannot inherit from
/// <see cref="Controllers.BaseApiController"/>.
/// </summary>
public sealed class CorrelationIdAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initialises a new instance of <see cref="CorrelationIdAccessor"/>.
    /// </summary>
    /// <param name="httpContextAccessor">ASP.NET Core HTTP context accessor.</param>
    public CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the correlation ID for the current request.
    /// Returns <see cref="string.Empty"/> if <see cref="Middleware.CorrelationIdMiddleware"/>
    /// has not yet run.
    /// </summary>
    public string CorrelationId =>
        _httpContextAccessor.HttpContext?.Items[Middleware.CorrelationIdMiddleware.ItemsKey]
            ?.ToString() ?? string.Empty;

    /// <summary>
    /// Gets a value indicating whether a non-empty correlation ID is available
    /// for the current request.
    /// </summary>
    public bool HasCorrelationId => !string.IsNullOrEmpty(CorrelationId);
}
