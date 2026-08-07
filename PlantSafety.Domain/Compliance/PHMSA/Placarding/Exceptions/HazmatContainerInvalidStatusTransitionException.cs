// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Domain
//  File:           HazmatContainerInvalidStatusTransitionException.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown when a HazmatContainer lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Placarding.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Placarding.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.HazmatContainer"/> lifecycle transition is not permitted.</summary>
public sealed class HazmatContainerInvalidStatusTransitionException : HazmatContainerDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public HazmatContainerInvalidStatusTransitionException(
        HazmatContainerId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition HazmatContainer '{id}' from '{from}' to '{to}'.",
            "HazmatContainer.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
