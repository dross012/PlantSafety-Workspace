// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Domain
//  File:           NearMissesLifecycleRules.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Guards for the NearMisses aggregate's lifecycle transitions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.NearMisses.Rules;

/// <summary>Defines permitted lifecycle transitions for the NearMisses aggregate.</summary>
public static class NearMissesLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when review may start from <paramref name="current"/>.</summary>
    public static bool CanStartReview(NearMissesStatus current) => current == NearMissesStatus.Reported;

    /// <summary>Returns <see langword="true"/> when the near-miss may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(NearMissesStatus current) => current == NearMissesStatus.UnderReview;

    /// <summary>Returns <see langword="true"/> when the near-miss may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(NearMissesStatus current) => current == NearMissesStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the near-miss may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(NearMissesStatus current) => current == NearMissesStatus.Closed;
}
