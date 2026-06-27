// ============================================================================
// File: ApiHeaderConstants.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: String constants for all custom HTTP header names used across
//              PlantSafety.Api middleware, filters, and controllers.
// ============================================================================
//
// Onboarding Notes:
// - Use these constants everywhere a header name appears as a string literal.
//   They are consumed by CorrelationIdMiddleware, TenantResolutionMiddleware,
//   ApiKeyAuthFilter, IdempotencyFilter, SecurityHeadersMiddleware, and all
//   OpenApi operation filters.
// - Adding a new custom header? Add its constant here first, then reference it
//   from the relevant middleware or filter. Never use inline string literals.
//
// Rationale:
// - Centralising header name strings prevents typos and ensures that OpenApi
//   documentation, middleware, and integration tests all reference the same values.
//
// Future Extension Seams:
// - Add X-Request-ID if a separate per-request tracing ID (distinct from
//   X-Correlation-ID) is required by the telemetry pipeline.
//
// ============================================================================

namespace PlantSafety.Api.Constants;

/// <summary>String constants for all custom HTTP headers used in PlantSafety.Api.</summary>
public static class ApiHeaderConstants
{
    /// <summary>
    /// Correlation ID header propagated through all service boundaries.
    /// Set by <see cref="Middleware.CorrelationIdMiddleware"/> on every request and response.
    /// </summary>
    public const string CorrelationId = "X-Correlation-ID";

    /// <summary>
    /// Tenant identifier header. Optional override — TenantId is normally resolved
    /// from the JWT <c>tenantId</c> claim by TenantResolutionMiddleware.
    /// </summary>
    public const string TenantId = "X-Tenant-ID";

    /// <summary>
    /// API key header for service-to-service authentication.
    /// Validated by <see cref="Filters.ApiKeyAuthFilter"/> on protected endpoints.
    /// </summary>
    public const string ApiKey = "X-API-Key";

    /// <summary>
    /// Idempotency key for POST operations. Clients supply a unique UUID v4/v7 per logical
    /// request so that duplicate submissions are detected and short-circuited.
    /// Consumed by <see cref="Filters.IdempotencyFilter"/>.
    /// </summary>
    public const string IdempotencyKey = "Idempotency-Key";

    /// <summary>
    /// API version deprecation notice header. Added to responses by
    /// ApiVersionDeprecationMiddleware when the requested version is sunsetted.
    /// </summary>
    public const string Deprecation = "Deprecation";

    /// <summary>
    /// Sunset date header indicating when a deprecated API version will be removed.
    /// Format: HTTP-date (RFC 7231 §7.1.1.1).
    /// </summary>
    public const string Sunset = "Sunset";
}
