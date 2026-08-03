// ============================================================================
// File: PredictiveSchedulingAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code modular-creation-agent)
// Created: 2026-08-03
// Description: Authorization policy name constants for the Scheduling.PredictiveScheduling
//              module's OptimizationRun request/history endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Referenced by [Authorize(Policy = ...)] on PredictiveSchedulingV1Controller.
// - Auto-discovered and registered by
//   PlantSafety.Api.Extensions.AuthorizationPolicyExtensions.AddPlantSafetyPolicies() via
//   reflection over every "*AuthorizationPolicies" class -- no additional registration required.
// ============================================================================

namespace PlantSafety.Api.Scheduling.Authorization;

/// <summary>Authorization policy name constants for the Scheduling.PredictiveScheduling module.</summary>
public static class PredictiveSchedulingAuthorizationPolicies
{
    /// <summary>Read access to OptimizationRun records/history. Granted to authorized roles.</summary>
    public const string Read = "RequirePredictiveSchedulingRead";

    /// <summary>Write access to request a new OptimizationRun. Granted to editor roles.</summary>
    public const string Write = "RequirePredictiveSchedulingWrite";
}
