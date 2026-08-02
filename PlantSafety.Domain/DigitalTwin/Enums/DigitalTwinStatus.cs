// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Domain
//  File:           DigitalTwinStatus.cs
//  Created:        2026-08-02
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the DigitalTwin aggregate root. Phase 4 full build-out.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.Enums;

/// <summary>Lifecycle states for the <see cref="PlantSafety.Domain.DigitalTwin.Aggregates.DigitalTwin"/> aggregate root.</summary>
public enum DigitalTwinStatus
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
