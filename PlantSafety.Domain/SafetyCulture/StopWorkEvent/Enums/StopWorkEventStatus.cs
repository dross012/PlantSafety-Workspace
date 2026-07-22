// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.StopWorkEvent
//  Layer:          Domain
//  File:           StopWorkEventStatus.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the StopWorkEvent aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.StopWorkEvent.Enums;

/// <summary>Lifecycle states for the <see cref="StopWorkEvent"/> aggregate root.</summary>
public enum StopWorkEventStatus
{
    /// <summary>Record has been created and awaits processing.</summary>
    Created = 0,

    /// <summary>Record is active and under management.</summary>
    Active = 1,

    /// <summary>Record has been completed or resolved.</summary>
    Completed = 2,

    /// <summary>Record has been closed with no further action required.</summary>
    Closed = 3,

    /// <summary>Record has been archived for long-term retention.</summary>
    Archived = 4,
}