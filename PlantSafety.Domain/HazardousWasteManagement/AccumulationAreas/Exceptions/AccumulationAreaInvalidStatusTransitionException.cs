// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.AccumulationAreas
//  Layer:          Domain
//  File:           AccumulationAreaInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Thrown when an AccumulationArea lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Exceptions;

/// <summary>Thrown when an AccumulationArea lifecycle transition is not permitted.</summary>
public sealed class AccumulationAreaInvalidStatusTransitionException : AccumulationAreaDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public AccumulationAreaInvalidStatusTransitionException(
        AccumulationAreaId id, string from, string to, CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'AccumulationArea' {id} from '{from}' to '{to}'.",
            "AccumulationArea.InvalidTransition", "Lifecycle", "Medium", correlationId, causationId)
    {
    }
}
