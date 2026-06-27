// ============================================================================
// File: TenantContextAccessor.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Presentation-layer accessor for tenant and site context stored in
//              HttpContext.Items by TenantResolutionMiddleware. Provides a typed
//              interface so controllers and filters do not reference raw dictionary keys.
// ============================================================================
//
// Onboarding Notes:
// - TenantResolutionMiddleware writes TenantId and SiteId into HttpContext.Items
//   using the key constants defined here. All other components should read via
//   this accessor, never by string literal.
// - IHttpContextAccessor is injected so this can be used in non-controller code
//   (filters, action results, output cache policies) that cannot inherit from
//   BaseApiController.
// - Returns Guid.Empty for TenantId and null for SiteId when the middleware has
//   not run (e.g. anonymous health-check endpoints). Callers should guard if
//   TenantId == Guid.Empty before issuing tenant-scoped queries.
//
// Rationale:
// - Centralising HttpContext.Items key names prevents the key string from being
//   scattered across the codebase and mismatched between the writer (middleware)
//   and readers (controllers, filters, output cache).
//
// Future Extension Seams:
// - Add CallerUserId: resolved from JWT "sub" claim for audit trail attribution.
// - Add ScopedPermissions: the resolved policy set cached per-request for fast
//   authorization checks without repeated JWT parsing.
//
// ============================================================================

namespace PlantSafety.Api.CrossDomain;

/// <summary>
/// Presentation-layer accessor for tenant and site identifiers stored in
/// <see cref="IHttpContextAccessor.HttpContext"/>.<see cref="HttpContext.Items"/>
/// by <see cref="Middleware.TenantResolutionMiddleware"/>.
/// </summary>
public sealed class TenantContextAccessor
{
    /// <summary>
    /// The <see cref="HttpContext.Items"/> key under which the resolved tenant ID is stored.
    /// Written by <see cref="Middleware.TenantResolutionMiddleware"/>.
    /// </summary>
    public const string TenantIdKey = "PlantSafety:TenantId";

    /// <summary>
    /// The <see cref="HttpContext.Items"/> key under which the resolved site ID is stored.
    /// Written by <see cref="Middleware.TenantResolutionMiddleware"/>.
    /// May be absent when a request is not site-scoped.
    /// </summary>
    public const string SiteIdKey = "PlantSafety:SiteId";

    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initialises a new instance of <see cref="TenantContextAccessor"/>.
    /// </summary>
    /// <param name="httpContextAccessor">ASP.NET Core HTTP context accessor.</param>
    public TenantContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the resolved tenant identifier for the current request.
    /// Returns <see cref="Guid.Empty"/> if <see cref="Middleware.TenantResolutionMiddleware"/>
    /// has not run (e.g. anonymous endpoints).
    /// </summary>
    public Guid TenantId =>
        _httpContextAccessor.HttpContext?.Items[TenantIdKey] is Guid g ? g : Guid.Empty;

    /// <summary>
    /// Gets the resolved site identifier for the current request, or <c>null</c>
    /// if the request is not site-scoped.
    /// </summary>
    public Guid? SiteId =>
        _httpContextAccessor.HttpContext?.Items[SiteIdKey] as Guid?;

    /// <summary>
    /// Gets a value indicating whether tenant context is available on the current request.
    /// </summary>
    public bool HasTenantContext => TenantId != Guid.Empty;
}
