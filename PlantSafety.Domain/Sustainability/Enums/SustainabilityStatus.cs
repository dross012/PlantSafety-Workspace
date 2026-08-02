// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Domain
//  File:           SustainabilityStatus.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle status vocabulary for the Sustainability aggregate root. Phase 4
//                  full build-out: generic status shape used across this build-out's modules.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.Enums;

/// <summary>Lifecycle states for the <see cref="Sustainability"/> aggregate root.</summary>
public enum SustainabilityStatus
{
    /// <summary>Initiative has been created and awaits activation.</summary>
    Created = 0,

    /// <summary>Initiative is active and under management.</summary>
    Active = 1,

    /// <summary>Initiative has been completed.</summary>
    Completed = 2,

    /// <summary>Initiative has been closed with no further action required.</summary>
    Closed = 3,

    /// <summary>Initiative has been archived for long-term retention.</summary>
    Archived = 4,
}
