// ============================================================================
// File: DocumentsAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Authorization policy name constants for the Documents module.
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

namespace PlantSafety.Api.Compliance.Policy.Documents.Authorization;

/// <summary>Authorization policy name constants for the Documents module.</summary>
public static class DocumentsAuthorizationPolicies
{
    /// <summary>Read access to Documents data. Granted to authorized roles.</summary>
    public const string Read  = "RequireCompliancePolicyDocumentsRead";

    /// <summary>Write access to Documents data. Granted to administrative roles.</summary>
    public const string Write = "RequireCompliancePolicyDocumentsWrite";
}