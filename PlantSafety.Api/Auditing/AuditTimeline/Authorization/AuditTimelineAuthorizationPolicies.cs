// ============================================================================
// File: AuditTimelineAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (API Contract Validation Agent)
// Created: 2026-08-02
// Last Updated: 2026-08-02
// Description: Authorization policy name constants for the Auditing.AuditTimeline sub-feature
//              (Phase 5 §6.1).
// ============================================================================
//
// Onboarding Notes:
// - Auto-discovered and registered by
//   PlantSafety.Api.Extensions.AuthorizationPolicyExtensions.AddPlantSafetyPolicies().
//
// ============================================================================

namespace PlantSafety.Api.Auditing.AuditTimeline.Authorization;

/// <summary>Authorization policy name constants for the Auditing.AuditTimeline sub-feature.</summary>
public static class AuditTimelineAuthorizationPolicies
{
    /// <summary>Read access to a reconstructed entity AuditTimeline. Granted to authorized roles.</summary>
    public const string Read = "RequireAuditingAuditTimelineRead";
}
