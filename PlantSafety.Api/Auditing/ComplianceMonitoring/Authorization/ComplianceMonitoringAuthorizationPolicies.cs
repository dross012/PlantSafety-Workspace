// ============================================================================
// File: ComplianceMonitoringAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Phase 7 §6.3 Support Tooling)
// Created: 2026-08-03
// Description: Authorization policy name constant for the Auditing.ComplianceMonitoring
//              module's V1 controller. The tenant-scoped access-log/permission-usage audit
//              stream is confidential, cross-cutting security data -- gated at the Approve
//              tier (ViewLogs), matching the platform's existing "audit/log viewing" precedent.
// ============================================================================
//
// Onboarding Notes:
// - Auto-discovered and registered by
//   PlantSafety.Api.Extensions.AuthorizationPolicyExtensions.AddPlantSafetyPolicies().
//
// ============================================================================

namespace PlantSafety.Api.Auditing.ComplianceMonitoring.Authorization;

/// <summary>Authorization policy name constants for the Auditing.ComplianceMonitoring module.</summary>
public static class ComplianceMonitoringAuthorizationPolicies
{
    /// <summary>Access to the tenant audit stream / permission-usage log. Approver-and-above.</summary>
    public const string ViewLogs = "RequireAuditingComplianceMonitoringViewLogs";
}
