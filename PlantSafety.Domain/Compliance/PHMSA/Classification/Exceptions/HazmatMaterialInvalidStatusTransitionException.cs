// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           HazmatMaterialInvalidStatusTransitionException.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Thrown when a HazmatMaterial lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Classification.Exceptions;

/// <summary>Thrown when a <c>HazmatMaterial</c> lifecycle transition is not permitted.</summary>
public sealed class HazmatMaterialInvalidStatusTransitionException : HazmatMaterialDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public HazmatMaterialInvalidStatusTransitionException(
        HazmatMaterialId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition HazmatMaterial '{id}' from '{from}' to '{to}'.",
            "HazmatMaterial.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
