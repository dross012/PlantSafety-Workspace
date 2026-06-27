// ============================================================================
// File: ISOAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Authorization policy name constants for the ISO module.
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

namespace PlantSafety.Api.Compliance.RegulatoryCalendar.Renewals.ISO.Authorization;

/// <summary>Authorization policy name constants for the ISO module.</summary>
public static class ISOAuthorizationPolicies
{
    /// <summary>Read access to ISO data. Granted to authorized roles.</summary>
    public const string Read  = "RequireComplianceRegCalISORead";

    /// <summary>Write access to ISO data. Granted to administrative roles.</summary>
    public const string Write = "RequireComplianceRegCalISOWrite";
}