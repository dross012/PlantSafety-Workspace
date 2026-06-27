// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Identity
//  Layer:          Infrastructure
//  File:           ActorContextResolver.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Resolves ActorId, TenantId, and SiteId from the validated ClaimsPrincipal on each request.
//                  Additive-only evolution.
// ============================================================================
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace PlantSafety.Infrastructure.Identity;

/// <summary>
/// Resolves the canonical actor context (ActorId, TenantId, SiteId) from the authenticated
/// <see cref="ClaimsPrincipal"/> bound to the current HTTP request.
/// </summary>
public sealed class ActorContextResolver(IHttpContextAccessor httpContextAccessor)
{
    private const string TenantIdClaim = "tid";
    private const string SiteIdClaim   = "extension_SiteId";

    /// <summary>Resolved actor (user or system) identity for the current request.</summary>
    public Guid ActorId   => GetGuidClaim(ClaimTypes.NameIdentifier);

    /// <summary>Resolved tenant identity for the current request.</summary>
    public Guid TenantId  => GetGuidClaim(TenantIdClaim);

    /// <summary>Resolved site identity for the current request.</summary>
    public Guid SiteId    => GetGuidClaim(SiteIdClaim);

    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    private Guid GetGuidClaim(string type)
    {
        var value = User?.FindFirstValue(type);
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }
}
