// ============================================================================
// File: LockoutTagoutProcedureAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Authorization policy name constants for the LockoutTagoutProcedure module.
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

namespace PlantSafety.Api.Compliance.Procedures.LockoutTagoutProcedure.Authorization;

/// <summary>Authorization policy name constants for the LockoutTagoutProcedure module.</summary>
public static class LockoutTagoutProcedureAuthorizationPolicies
{
    /// <summary>Read access to LockoutTagoutProcedure data. Granted to authorized roles.</summary>
    public const string Read  = "RequireComplianceProceduresLockoutTagoutProcedureRead";

    /// <summary>Write access to LockoutTagoutProcedure data. Granted to administrative roles.</summary>
    public const string Write = "RequireComplianceProceduresLockoutTagoutProcedureWrite";
}