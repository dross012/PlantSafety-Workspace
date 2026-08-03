// ============================================================================
// File: TenantAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
// Created: 2026-08-03
// Description: Authorization policy name constants for the Tenancy.Tenant module. Every
//              operation here -- including read (Get/List) -- exposes cross-tenant registry data
//              (another tenant's existence, plan, status) or mutates the most consequential
//              administrative surface in the platform (provisioning/suspending/archiving/
//              deleting entire tenants), so every action is gated under the single "Manage"
//              field-name convention, which AuthorizationPolicyExtensions.TierForFieldName
//              resolves to the strictest SafetyRoles.AdminOnly (PlatformAdmin-and-above) tier --
//              there is no lower "Viewer" tier for cross-tenant visibility in this platform.
// ============================================================================
//
// Onboarding Notes:
// - Auto-discovered and registered by
//   PlantSafety.Api.Extensions.AuthorizationPolicyExtensions.AddPlantSafetyPolicies().
//
// ============================================================================

namespace PlantSafety.Api.Tenancy.Authorization;

/// <summary>Authorization policy name constants for the Tenancy.Tenant module.</summary>
public static class TenantAuthorizationPolicies
{
    /// <summary>Access to every Tenancy.Tenant operation (read and write alike). Platform-admin-only -- see file header.</summary>
    public const string Manage = "RequireTenancyTenantManage";
}
