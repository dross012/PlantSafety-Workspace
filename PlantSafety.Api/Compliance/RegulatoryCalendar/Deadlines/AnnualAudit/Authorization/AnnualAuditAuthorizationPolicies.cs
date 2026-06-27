// ============================================================================
// File: AnnualAuditAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Authorization policy name constants for the AnnualAudit module.
// ============================================================================
//
// Onboarding Notes:
// - Referenced by [Authorize(Policy = ...)] on controllers and AddPlantSafetyAuthorization() in ServiceCollectionExtensions.
//
// Rationale:
// - Centralises policy names as constants to prevent magic-string drift across controllers and DI registration.
//
// Future Extension Seams:
// - Add granular per-operation policies (e.g. RequireExport) if export endpoints are added.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.RegulatoryCalendar.Deadlines.AnnualAudit.Authorization;

/// <summary>Authorization policy name constants for the AnnualAudit module.</summary>
public static class AnnualAuditAuthorizationPolicies
{
    /// <summary>Read access to AnnualAudit data. Granted to authorized roles.</summary>
    public const string Read  = "RequireComplianceRegCalAnnualAuditRead";

    /// <summary>Write access to AnnualAudit data. Granted to administrative roles.</summary>
    public const string Write = "RequireComplianceRegCalAnnualAuditWrite";
}