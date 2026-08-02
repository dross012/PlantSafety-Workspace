// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Domain
//  File:           WorkerWellBeingStatus.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle status vocabulary for the WorkerWellBeing aggregate root. Phase 4
//                  full build-out: generic status shape used across this build-out's modules.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.WorkerWellBeing.Enums;

/// <summary>Lifecycle states for the <see cref="WorkerWellBeing"/> aggregate root.</summary>
public enum WorkerWellBeingStatus
{
    /// <summary>Record has been created and awaits activation.</summary>
    Created = 0,

    /// <summary>Record is active and under management.</summary>
    Active = 1,

    /// <summary>Record has been completed.</summary>
    Completed = 2,

    /// <summary>Record has been closed with no further action required.</summary>
    Closed = 3,

    /// <summary>Record has been archived for long-term retention.</summary>
    Archived = 4,
}
