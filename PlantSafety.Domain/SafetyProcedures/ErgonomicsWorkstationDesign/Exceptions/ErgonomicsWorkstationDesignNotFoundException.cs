// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.ErgonomicsWorkstationDesign
//  Layer:          Domain
//  File:           ErgonomicsWorkstationDesignNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a ErgonomicsWorkstationDesign aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Exceptions;

/// <summary>Thrown when a <see cref="ErgonomicsWorkstationDesign"/> aggregate lookup finds no match.</summary>
public sealed class ErgonomicsWorkstationDesignNotFoundException : ErgonomicsWorkstationDesignDomainException
{
    /// <summary>Creates a not-found exception for the given ErgonomicsWorkstationDesign identity.</summary>
    public ErgonomicsWorkstationDesignNotFoundException(ErgonomicsWorkstationDesignId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "ErgonomicsWorkstationDesign with id '{id}' was not found.",
            "ErgonomicsWorkstationDesign.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}