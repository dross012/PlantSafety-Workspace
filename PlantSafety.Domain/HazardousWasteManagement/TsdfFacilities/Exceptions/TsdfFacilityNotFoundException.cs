// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.TsdfFacilities
//  Layer:          Domain
//  File:           TsdfFacilityNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Thrown when a TsdfFacility aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Exceptions;

/// <summary>Thrown when a TsdfFacility aggregate lookup finds no match.</summary>
public sealed class TsdfFacilityNotFoundException : TsdfFacilityDomainException
{
    /// <summary>Creates a not-found exception for the given TsdfFacility identity.</summary>
    public TsdfFacilityNotFoundException(TsdfFacilityId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"TsdfFacility with id '{id}' was not found.",
            "TsdfFacility.NotFound", "NotFound", "Medium", correlationId, causationId)
    {
    }
}
