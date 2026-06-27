// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Identity
//  Layer:          Infrastructure
//  File:           ClaimsPrincipalExtensions.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Extension methods on ClaimsPrincipal for extracting PlantSafety canonical claims.
//                  Additive-only evolution.
// ============================================================================
using System.Security.Claims;

namespace PlantSafety.Infrastructure.Identity;

/// <summary>Extension methods on <see cref="ClaimsPrincipal"/> for PlantSafety canonical claims.</summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>Returns the Azure AD object ID as the actor identity.</summary>
    public static Guid GetActorId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? principal.FindFirstValue("oid");
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }

    /// <summary>Returns the Azure AD tenant ID.</summary>
    public static Guid GetTenantId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue("tid");
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }

    /// <summary>Returns the site ID from a custom extension claim.</summary>
    public static Guid GetSiteId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue("extension_SiteId");
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }

    /// <summary>Returns <see langword="true"/> when the principal has the given role claim.</summary>
    public static bool HasRole(this ClaimsPrincipal principal, string role)
        => principal.IsInRole(role);
}
