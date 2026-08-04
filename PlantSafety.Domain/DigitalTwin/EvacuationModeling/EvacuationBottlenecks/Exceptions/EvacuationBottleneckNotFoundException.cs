// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
//  Layer:          Domain
//  File:           EvacuationBottleneckNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an EvacuationBottleneck aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Exceptions;

/// <summary>Thrown when an <see cref="Aggregates.EvacuationBottleneck"/> aggregate lookup finds no match.</summary>
public sealed class EvacuationBottleneckNotFoundException : EvacuationBottleneckDomainException
{
    /// <summary>Creates a not-found exception for the given EvacuationBottleneck identity.</summary>
    public EvacuationBottleneckNotFoundException(EvacuationBottleneckId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"EvacuationBottleneck with id '{id}' was not found.",
            "EvacuationBottleneck.NotFound", "NotFound", "Medium",
            correlationId, causationId)
    {
    }
}
