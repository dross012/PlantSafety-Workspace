// ============================================================================
// File: MaintenanceAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Authorization policy name constants for the Maintenance module.
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

namespace PlantSafety.Api.Compliance.Procedures.Maintenance.Authorization;

/// <summary>Authorization policy name constants for the Maintenance module.</summary>
public static class MaintenanceAuthorizationPolicies
{
    /// <summary>Read access to Maintenance data. Granted to authorized roles.</summary>
    public const string Read  = "RequireComplianceProceduresMaintenanceRead";

    /// <summary>Write access to Maintenance data. Granted to administrative roles.</summary>
    public const string Write = "RequireComplianceProceduresMaintenanceWrite";
}