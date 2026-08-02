// ============================================================================
// File: AIInvestigationAuthorizationPolicies.cs
// Project: PlantSafety.Api
// Module: Intelligence.AIInvestigations
// Created: 2026-08-02
// Description: Authorization policy name constants for the Intelligence.AIInvestigations module
//              (the new DDD write-side aggregate).
// ============================================================================

namespace PlantSafety.Api.Intelligence.AIInvestigations.Authorization;

/// <summary>Authorization policy name constants for the AIInvestigation aggregate module.</summary>
public static class AIInvestigationAuthorizationPolicies
{
    /// <summary>Read access to AIInvestigation data. Granted to authorized roles.</summary>
    public const string Read = "RequireIntelligenceAIInvestigationRead";

    /// <summary>Write access to AIInvestigation data. Granted to administrative roles.</summary>
    public const string Write = "RequireIntelligenceAIInvestigationWrite";
}
