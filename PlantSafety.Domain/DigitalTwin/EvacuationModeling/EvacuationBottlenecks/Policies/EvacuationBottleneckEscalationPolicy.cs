// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
//  Layer:          Domain
//  File:           EvacuationBottleneckEscalationPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing when a detected EvacuationBottleneck should escalate
//                  via AlertRoutingService (risk hook). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Policies;

/// <summary>Determines whether an <see cref="Aggregates.EvacuationBottleneck"/> should escalate via AlertRoutingService.</summary>
public static class EvacuationBottleneckEscalationPolicy
{
    /// <summary>Recurrence count at or above which a bottleneck is treated as chronic and must escalate.</summary>
    public const int ChronicRecurrenceThreshold = 3;

    /// <summary>Returns <see langword="true"/> when the bottleneck should escalate (unmitigated and either newly detected or chronic).</summary>
    public static bool ShouldEscalate(Aggregates.EvacuationBottleneck aggregate)
        => aggregate.Status != EvacuationBottleneckStatus.Mitigated
           && (aggregate.Status == EvacuationBottleneckStatus.Detected || aggregate.RecurrenceCount >= ChronicRecurrenceThreshold);
}
