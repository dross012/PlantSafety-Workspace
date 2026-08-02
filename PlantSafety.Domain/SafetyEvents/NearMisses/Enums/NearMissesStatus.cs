// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Domain
//  File:           NearMissesStatus.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the NearMisses aggregate root. Matches the vocabulary
//                  already documented on the pre-existing read-model DTO's Status property
//                  (e.g., Reported/UnderReview/Closed).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.NearMisses.Enums;

/// <summary>Lifecycle states for the NearMisses aggregate root.</summary>
public enum NearMissesStatus
{
    /// <summary>A near-miss has been reported and awaits review.</summary>
    Reported = 0,

    /// <summary>A reviewer has started assessing the near-miss.</summary>
    UnderReview = 1,

    /// <summary>The review is complete and findings have been recorded.</summary>
    Completed = 2,

    /// <summary>The near-miss has been formally closed.</summary>
    Closed = 3,

    /// <summary>The near-miss has been archived for compliance retention.</summary>
    Archived = 4,
}
