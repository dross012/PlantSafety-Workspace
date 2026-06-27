// ============================================================================
// File: ISO14001AuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Authorization policy name constants for the ISO14001 module.
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

namespace PlantSafety.Api.Compliance.Certification.ISO14001.Authorization;

/// <summary>Authorization policy name constants for the ISO14001 module.</summary>
public static class ISO14001AuthorizationPolicies
{
    /// <summary>Read access to ISO14001 data. Granted to authorized roles.</summary>
    public const string Read  = "RequireComplianceCertificationISO14001Read";

    /// <summary>Write access to ISO14001 data. Granted to administrative roles.</summary>
    public const string Write = "RequireComplianceCertificationISO14001Write";
}