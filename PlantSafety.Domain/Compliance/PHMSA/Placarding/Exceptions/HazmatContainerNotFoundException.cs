// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Domain
//  File:           HazmatContainerNotFoundException.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown when a HazmatContainer aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Placarding.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Placarding.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.HazmatContainer"/> aggregate lookup finds no match.</summary>
public sealed class HazmatContainerNotFoundException : HazmatContainerDomainException
{
    /// <summary>Creates a not-found exception for the given HazmatContainer identity.</summary>
    public HazmatContainerNotFoundException(HazmatContainerId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"HazmatContainer with id '{id}' was not found.",
            "HazmatContainer.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
