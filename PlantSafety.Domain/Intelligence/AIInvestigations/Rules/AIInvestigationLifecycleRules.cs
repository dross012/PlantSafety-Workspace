// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Domain
//  File:           AIInvestigationLifecycleRules.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Guards for the AIInvestigation aggregate's lifecycle transitions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.AIInvestigations.Rules;

/// <summary>Defines permitted lifecycle transitions for the AIInvestigation aggregate.</summary>
public static class AIInvestigationLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when review may be started from <paramref name="current"/>.</summary>
    public static bool CanStartReview(AIInvestigationStatus current) => current == AIInvestigationStatus.Open;

    /// <summary>Returns <see langword="true"/> when the investigation may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(AIInvestigationStatus current) => current == AIInvestigationStatus.InProgress;

    /// <summary>Returns <see langword="true"/> when the investigation may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(AIInvestigationStatus current) => current == AIInvestigationStatus.Closed;
}
