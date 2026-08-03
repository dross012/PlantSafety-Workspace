// ============================================================================
// File: AuditReportAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (API Contract Validation Agent)
// Created: 2026-08-02
// Last Updated: 2026-08-02
// Description: Authorization policy name constants for the Auditing.AuditReport sub-feature
//              (Phase 5 §6.2).
// ============================================================================
//
// Onboarding Notes:
// - Auto-discovered and registered by
//   PlantSafety.Api.Extensions.AuthorizationPolicyExtensions.AddPlantSafetyPolicies().
//
// ============================================================================

namespace PlantSafety.Api.Auditing.AuditReport.Authorization;

/// <summary>Authorization policy name constants for the Auditing.AuditReport sub-feature.</summary>
public static class AuditReportAuthorizationPolicies
{
    /// <summary>Read access to generated AuditReports and their history/content. Granted to authorized roles.</summary>
    public const string Read = "RequireAuditingAuditReportRead";

    /// <summary>Access to generate a new AuditReport. Granted to editor roles.</summary>
    public const string Write = "RequireAuditingAuditReportWrite";

    /// <summary>Access to download rendered AuditReport content. Granted to approver-and-above roles per export sensitivity.</summary>
    public const string Export = "RequireAuditingAuditReportExport";
}
