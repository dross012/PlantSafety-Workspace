// ============================================================================
// File: ImpersonationAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Phase 7 §6.3 Support Tooling)
// Created: 2026-08-03
// Description: Authorization policy name constant for the Support.Impersonation module's V1
//              controller. Starting/ending a tenant-impersonation session is the single most
//              security-sensitive operator action in the Admin Portal, so it is gated
//              PlatformAdmin-only (SafetyRoles.AdminOnly), matching TenantAuthorizationPolicies.Manage.
// ============================================================================
//
// Onboarding Notes:
// - Auto-discovered and registered by
//   PlantSafety.Api.Extensions.AuthorizationPolicyExtensions.AddPlantSafetyPolicies().
//
// ============================================================================

namespace PlantSafety.Api.Support.Impersonation.Authorization;

/// <summary>Authorization policy name constants for the Support.Impersonation module.</summary>
public static class ImpersonationAuthorizationPolicies
{
    /// <summary>Access to every impersonation-session operation. Platform-admin-only -- see file header.</summary>
    public const string Manage = "RequireSupportImpersonationManage";
}
