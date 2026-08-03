// ============================================================================
// File: BillingAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Phase 7 §6.1 Admin Portal / Billing)
// Created: 2026-08-03
// Description: Authorization policy name constant for the Billing bounded context's V1
//              controller. Billing exposes tenant subscription/invoice/usage data and
//              mutates invoices (generate/charge/refund) -- a platform-admin-only surface,
//              matching the same rationale as TenantAuthorizationPolicies.Manage.
// ============================================================================
//
// Onboarding Notes:
// - Auto-discovered and registered by
//   PlantSafety.Api.Extensions.AuthorizationPolicyExtensions.AddPlantSafetyPolicies().
//
// ============================================================================

namespace PlantSafety.Api.Billing.Authorization;

/// <summary>Authorization policy name constants for the Billing module.</summary>
public static class BillingAuthorizationPolicies
{
    /// <summary>Access to every Billing operation (read and write alike). Platform-admin-only -- see file header.</summary>
    public const string Manage = "RequireBillingManage";
}
