// ============================================================================
// File: GlobalSafetyDashboardAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Phase 8 §5 Global Safety Dashboard)
// Created: 2026-08-03
// Description: Authorization policy name constant for the GlobalSafetyDashboard bounded
//              context's V1 controller. The Global Safety Dashboard is an Admin/Executive-only
//              surface exposing cross-site/cross-tenant safety intelligence (KPIs, risk scores,
//              anomalies, recommendations, cross-site coordinated-action audit trail) -- the
//              spec explicitly requires Approver/Admin-tier gating for the entire controller,
//              matching the "Approve" verb -> SafetyRoles.ApproverAndAbove mapping already
//              established by AuthorizationPolicyExtensions.TierForFieldName.
// ============================================================================
//
// Onboarding Notes:
// - Auto-discovered and registered by
//   PlantSafety.Api.Extensions.AuthorizationPolicyExtensions.AddPlantSafetyPolicies().
// - The "Approve" field name maps to SafetyRoles.ApproverAndAbove (Approver + PlatformAdmin),
//   which is exactly the "Approver/Admin tier" the spec requires -- see
//   AuthorizationPolicyExtensions.TierForFieldName.
//
// ============================================================================

namespace PlantSafety.Api.GlobalSafetyInsights.Authorization;

/// <summary>Authorization policy name constants for the Global Safety Dashboard module.</summary>
public static class GlobalSafetyDashboardAuthorizationPolicies
{
    /// <summary>Access to every Global Safety Dashboard operation. Approver/Admin-tier only -- see file header.</summary>
    public const string Approve = "RequireGlobalSafetyDashboardApprove";
}
