// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
//  Layer:          Domain
//  File:           EvacuationBottleneckStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the EvacuationBottleneck aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Enums;

/// <summary>Lifecycle states for the <see cref="Aggregates.EvacuationBottleneck"/> aggregate root.</summary>
public enum EvacuationBottleneckStatus
{
    /// <summary>Bottleneck detected and not yet mitigated.</summary>
    Detected = 0,

    /// <summary>Bottleneck detected again on a subsequent simulation run after being previously detected.</summary>
    Recurred = 1,

    /// <summary>Bottleneck has been mitigated (corrective action applied).</summary>
    Mitigated = 2,
}
