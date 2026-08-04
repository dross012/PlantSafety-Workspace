// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.AccumulationAreas
//  Layer:          Domain
//  File:           AccumulationAreaNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Thrown when an AccumulationArea aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Exceptions;

/// <summary>Thrown when an AccumulationArea aggregate lookup finds no match.</summary>
public sealed class AccumulationAreaNotFoundException : AccumulationAreaDomainException
{
    /// <summary>Creates a not-found exception for the given AccumulationArea identity.</summary>
    public AccumulationAreaNotFoundException(AccumulationAreaId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"AccumulationArea with id '{id}' was not found.",
            "AccumulationArea.NotFound", "NotFound", "Medium", correlationId, causationId)
    {
    }
}
