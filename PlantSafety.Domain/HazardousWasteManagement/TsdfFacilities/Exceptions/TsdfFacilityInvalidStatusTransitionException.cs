// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.TsdfFacilities
//  Layer:          Domain
//  File:           TsdfFacilityInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Thrown when a TsdfFacility lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Exceptions;

/// <summary>Thrown when a TsdfFacility lifecycle transition is not permitted.</summary>
public sealed class TsdfFacilityInvalidStatusTransitionException : TsdfFacilityDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public TsdfFacilityInvalidStatusTransitionException(
        TsdfFacilityId id, string from, string to, CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'TsdfFacility' {id} from '{from}' to '{to}'.",
            "TsdfFacility.InvalidTransition", "Lifecycle", "Medium", correlationId, causationId)
    {
    }
}
