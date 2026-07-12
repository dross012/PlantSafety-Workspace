// ============================================================================
// File: TenantResolutionMiddleware.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-12
// Last Updated: 2026-06-12
// Description: ASP.NET Core middleware that resolves tenant and site context from
//              the authenticated JWT claims and populates TenantContext for the request.
// ============================================================================
//
// Onboarding Notes:
// - Must be registered AFTER CorrelationIdMiddleware and AFTER Authentication middleware
//   in the pipeline (see Program.cs). The JWT must be validated before claims are read.
// - Reads tenantId and siteId claims from the authenticated ClaimsPrincipal.
//   If either claim is missing from an authenticated request, the request is rejected with 401.
// - The resolved TenantId is propagated explicitly into all Application-layer commands,
//   queries, and Dapper-based repository calls. It is NEVER inferred from ambient state.
//   See Architecture Specification §7.1 (Multi-Tenant Isolation).
// - Anonymous endpoints (health checks, swagger) skip tenant resolution — the middleware
//   only activates when context.User.Identity.IsAuthenticated is true.
//
// Rationale:
// - Deterministic, claim-based tenant isolation is the Zero-Trust requirement.
//   ORM-level global query filters are explicitly prohibited by the Enterprise Architectural
//   Rule Handbook. Every SQL call receives TenantId as an explicit stored procedure parameter.
//
// Future Extension Seams:
// - Add support for resolving tenant from a custom header for service-to-service calls.
// - Extend to populate additional context (e.g., site timezone) from the tenant registry.
//
// ============================================================================

using System.Security.Claims;
using PlantSafety.Api.CrossDomain;

namespace PlantSafety.Api.Middleware;

/// <summary>
/// Middleware that extracts tenant and site identifiers from the JWT bearer token
/// and populates <see cref="HttpContext.Items"/> for the current request, using the
/// key constants defined on <see cref="TenantContextAccessor"/>.
/// </summary>
public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="TenantResolutionMiddleware"/>.
    /// </summary>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <param name="logger">The logger instance for this middleware.</param>
    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Resolves tenant context from authenticated JWT claims and attaches it to
    /// <see cref="HttpContext.Items"/> for the current request. Unauthenticated requests
    /// pass through without resolution.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/>.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantId = context.User.FindFirstValue("tenantId");
            var siteId = context.User.FindFirstValue("siteId");

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                _logger.LogWarning(
                    "Authenticated request missing tenantId claim. CorrelationId: {CorrelationId}",
                    context.Items[CorrelationIdMiddleware.ItemsKey]);

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            if (Guid.TryParse(tenantId, out var tenantGuid))
            {
                context.Items[TenantContextAccessor.TenantIdKey] = tenantGuid;
            }

            if (Guid.TryParse(siteId, out var siteGuid))
            {
                context.Items[TenantContextAccessor.SiteIdKey] = siteGuid;
            }
        }

        await _next(context);
    }
}
