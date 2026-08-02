// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Domain
//  File:           AIInvestigationStatus.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the AIInvestigation aggregate root. Matches the vocabulary
//                  documented on the pre-existing AllInvestigationsItemDto.Status property
//                  ("Open/InProgress/Closed") plus the platform-standard Archived terminal state.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.AIInvestigations.Enums;

/// <summary>Lifecycle states for the AIInvestigation aggregate root.</summary>
public enum AIInvestigationStatus
{
    /// <summary>The investigation has been opened and awaits review.</summary>
    Open = 0,

    /// <summary>The investigation is under active review.</summary>
    InProgress = 1,

    /// <summary>The investigation has been closed.</summary>
    Closed = 2,

    /// <summary>The investigation has been archived for compliance retention.</summary>
    Archived = 3,
}
