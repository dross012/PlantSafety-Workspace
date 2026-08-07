// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           HazmatMaterialNotFoundException.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown when a HazmatMaterial aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Classification.Exceptions;

/// <summary>Thrown when a <c>HazmatMaterial</c> aggregate lookup finds no match.</summary>
public sealed class HazmatMaterialNotFoundException : HazmatMaterialDomainException
{
    /// <summary>Creates a not-found exception for the given HazmatMaterial identity.</summary>
    public HazmatMaterialNotFoundException(HazmatMaterialId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"HazmatMaterial with id '{id}' was not found.",
            "HazmatMaterial.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
