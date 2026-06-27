// ============================================================================
// File: ComplianceAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Authorization policy name constants for the Compliance module.
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

namespace PlantSafety.Api.Analytics.ScoreCards.Compliance.Authorization;

/// <summary>Authorization policy name constants for the Compliance module.</summary>
public static class ComplianceAuthorizationPolicies
{
    /// <summary>Read access to Compliance data. Granted to authorized roles.</summary>
    public const string Read  = "RequireAnalyticsScoreCardsComplianceRead";

    /// <summary>Write access to Compliance data. Granted to administrative roles.</summary>
    public const string Write = "RequireAnalyticsScoreCardsComplianceWrite";
}