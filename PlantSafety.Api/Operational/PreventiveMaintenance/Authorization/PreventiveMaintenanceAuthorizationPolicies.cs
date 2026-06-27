// ============================================================================
// File: PreventiveMaintenanceAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Authorization policy name constants for the PreventiveMaintenance module.
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

namespace PlantSafety.Api.Operational.PreventiveMaintenance.Authorization;

/// <summary>Authorization policy name constants for the PreventiveMaintenance module.</summary>
public static class PreventiveMaintenanceAuthorizationPolicies
{
    /// <summary>Read access to PreventiveMaintenance data. Granted to authorized roles.</summary>
    public const string Read  = "RequireOperationalPreventiveMaintenanceRead";

    /// <summary>Write access to PreventiveMaintenance data. Granted to administrative roles.</summary>
    public const string Write = "RequireOperationalPreventiveMaintenanceWrite";
}