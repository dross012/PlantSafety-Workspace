// ============================================================================
// File: TenantResolutionMiddleware.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-12
// Last Updated: 2026-08-03 -- Phase 8 alignment governance fix. A platform-wide audit found
//   that the UI's ImpersonationHeaderHandler stamped X-Tenant-ID on every outbound request
//   during an active, audited, time-boxed impersonation session, but this middleware never read
//   any header -- the "browse as tenant" admin feature was functionally inert (failed closed: no
//   unauthorized access occurred, the header was simply ignored, but impersonation did nothing).
//   Fixed by honoring X-Tenant-ID ONLY when accompanied by a valid X-Impersonation-Session-Id
//   naming a real server-side session record that (a) belongs to the authenticated caller,
//   (b) was started by a caller holding Platform.Admin, (c) is still active (not ended, not past
//   its 30-minute time-box), and (d) is bound to this exact tenant. Any failed check silently
//   ignores both headers and falls back to the JWT tenantId claim -- there is no header-only
//   tenant switch under any circumstance.
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
// - Impersonation override (Phase 8 fix): X-Tenant-ID is trusted ONLY after ICacheService
//   confirms a live ImpersonationSessionDto for the X-Impersonation-Session-Id header, owned by
//   the caller's own "sub"/NameIdentifier claim, started by a caller holding Platform.Admin, and
//   bound to the exact tenant named by X-Tenant-ID. All four checks happen server-side, every
//   request -- the client never supplies anything the server doesn't independently re-verify.
//
// Rationale:
// - Deterministic, claim-based tenant isolation is the Zero-Trust requirement.
//   ORM-level global query filters are explicitly prohibited by the Enterprise Architectural
//   Rule Handbook. Every SQL call receives TenantId as an explicit stored procedure parameter.
//
// Future Extension Seams:
// - Extend to populate additional context (e.g., site timezone) from the tenant registry.
//
// ============================================================================

using System.Security.Claims;
using PlantSafety.Api.CrossDomain;
using PlantSafety.Api.Constants;
using PlantSafety.Application.Abstractions.Caching;
using PlantSafety.Application.Support.Impersonation.Models;
using Shared.Authorization.Abstractions.Roles;

namespace PlantSafety.Api.Middleware;

/// <summary>
/// Middleware that extracts tenant and site identifiers from the JWT bearer token
/// and populates <see cref="HttpContext.Items"/> for the current request, using the
/// key constants defined on <see cref="TenantContextAccessor"/>. Optionally overrides the
/// resolved TenantId with an impersonated tenant, but only after independently validating a
/// real, active, correctly-owned, correctly-bound impersonation session server-side.
/// </summary>
public sealed class TenantResolutionMiddleware
{
    private const string UserIdClaimType = "sub";
    private const string RoleClaimType = "role";

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
    /// <param name="cacheService">
    /// Resolved per-request from the DI container (middleware method injection) to look up an
    /// impersonation session record, if an impersonation override is being attempted.
    /// </param>
    public async Task InvokeAsync(HttpContext context, ICacheService cacheService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantId = context.User.FindFirstValue("tenantId");
            var siteId = context.User.FindFirstValue("siteId");
            var region = context.User.FindFirstValue("region");

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

            // Phase 7 §1.2 Tenant Context Enforcement: resolves the tenant's data-residency/
            // deployment region from the same JWT the tenantId/siteId claims come from -- never
            // client-supplied via query/body -- mirroring the tenantId/siteId resolution pattern
            // above. Optional: absent for tenants provisioned before region tracking existed.
            if (!string.IsNullOrWhiteSpace(region))
            {
                context.Items[TenantContextAccessor.RegionKey] = region;
            }

            // Phase 8 alignment fix: strictly-validated tenant-impersonation override. Both
            // headers must be present and every check below must pass, or the header is ignored
            // entirely and the JWT-derived tenant (already set above) stands.
            var impersonatedTenantId = await TryResolveImpersonationOverrideAsync(context, cacheService);
            if (impersonatedTenantId is not null)
            {
                context.Items[TenantContextAccessor.TenantIdKey] = impersonatedTenantId.Value;
            }
        }

        await _next(context);
    }

    /// <summary>
    /// Validates an impersonation override attempt against the real, server-side session record.
    /// Returns the impersonated TenantId only when every check passes; otherwise returns
    /// <see langword="null"/> and the caller's JWT-derived tenant is used unchanged. Never throws
    /// on a missing/invalid/expired/mismatched session -- an invalid override attempt is treated
    /// identically to no override attempt at all.
    /// </summary>
    private async Task<Guid?> TryResolveImpersonationOverrideAsync(HttpContext context, ICacheService cacheService)
    {
        var requestedTenantIdHeader = context.Request.Headers[ApiHeaderConstants.TenantId].ToString();
        var sessionIdHeader = context.Request.Headers[ApiHeaderConstants.ImpersonationSessionId].ToString();

        if (string.IsNullOrWhiteSpace(requestedTenantIdHeader) || string.IsNullOrWhiteSpace(sessionIdHeader))
        {
            return null;
        }

        var correlationId = context.Items[CorrelationIdMiddleware.ItemsKey];

        if (!Guid.TryParse(requestedTenantIdHeader, out var requestedTenantId) ||
            !Guid.TryParse(sessionIdHeader, out var sessionId))
        {
            _logger.LogWarning(
                "Impersonation override ignored: malformed X-Tenant-ID/X-Impersonation-Session-Id header. CorrelationId: {CorrelationId}",
                correlationId);
            return null;
        }

        // Check 1: caller holds Platform.Admin. Mirrors CurrentUserService.IsInRole's dual claim
        // check ("role" claim type primary, ClaimTypes.Role fallback) since this platform's JWTs
        // are not guaranteed to use the same claim type ASP.NET Core's IsInRole() checks by default.
        var isPlatformAdmin =
            context.User.FindAll(RoleClaimType).Any(c => string.Equals(c.Value, SafetyRoles.PlatformAdmin, StringComparison.OrdinalIgnoreCase)) ||
            context.User.IsInRole(SafetyRoles.PlatformAdmin);

        if (!isPlatformAdmin)
        {
            _logger.LogWarning(
                "Impersonation override rejected: caller does not hold Platform.Admin. CorrelationId: {CorrelationId}",
                correlationId);
            return null;
        }

        // Check 2: resolve the caller's own UserId (never trust a header for this either).
        var callerUserIdClaim = context.User.FindFirstValue(UserIdClaimType) ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(callerUserIdClaim, out var callerUserId))
        {
            _logger.LogWarning(
                "Impersonation override rejected: caller has no resolvable UserId claim. CorrelationId: {CorrelationId}",
                correlationId);
            return null;
        }

        // Check 3: the named session must be a real, live record -- looked up server-side, never
        // trusted from the request. A missing/expired/miss on the distributed cache is
        // indistinguishable from "no session" here by design (fail closed).
        var session = await cacheService.GetAsync<ImpersonationSessionDto>(ImpersonationCacheKeys.ForSession(sessionId), context.RequestAborted);
        if (session is null || !session.IsActive(DateTime.UtcNow))
        {
            _logger.LogWarning(
                "Impersonation override rejected: session {SessionId} not found or not active. CorrelationId: {CorrelationId}",
                sessionId, correlationId);
            return null;
        }

        // Check 4: the session must belong to THIS caller (an admin cannot use another admin's
        // session id, even a live one) and must be bound to the EXACT tenant the request claims.
        if (session.AdminUserId != callerUserId || session.TenantId != requestedTenantId)
        {
            _logger.LogWarning(
                "Impersonation override rejected: session {SessionId} ownership/tenant-binding mismatch. CorrelationId: {CorrelationId}",
                sessionId, correlationId);
            return null;
        }

        _logger.LogInformation(
            "Impersonation override applied: caller {CallerUserId} operating as Tenant {TenantId} via session {SessionId}. CorrelationId: {CorrelationId}",
            callerUserId, requestedTenantId, sessionId, correlationId);

        return requestedTenantId;
    }
}
