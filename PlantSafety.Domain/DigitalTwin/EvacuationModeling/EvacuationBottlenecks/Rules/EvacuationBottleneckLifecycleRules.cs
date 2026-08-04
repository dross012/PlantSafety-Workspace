// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
//  Layer:          Domain
//  File:           EvacuationBottleneckLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the EvacuationBottleneck aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.EvacuationBottleneck"/> aggregate.</summary>
public static class EvacuationBottleneckLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when a recurrence may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordRecurrence(EvacuationBottleneckStatus current) =>
        current is EvacuationBottleneckStatus.Detected or EvacuationBottleneckStatus.Recurred;

    /// <summary>Returns <see langword="true"/> when the bottleneck may be marked mitigated from <paramref name="current"/>.</summary>
    public static bool CanMarkMitigated(EvacuationBottleneckStatus current) => current != EvacuationBottleneckStatus.Mitigated;
}
