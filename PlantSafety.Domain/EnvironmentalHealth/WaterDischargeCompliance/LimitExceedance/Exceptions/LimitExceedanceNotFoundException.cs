// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
//  Layer:          Domain
//  File:           LimitExceedanceNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a LimitExceedance aggregate lookup finds no match. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Exceptions;

/// <summary>Thrown when a <see cref="LimitExceedance"/> aggregate lookup finds no match.</summary>
public sealed class LimitExceedanceNotFoundException : LimitExceedanceDomainException
{
    /// <summary>Creates a not-found exception for the given LimitExceedance identity.</summary>
    public LimitExceedanceNotFoundException(LimitExceedanceId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"LimitExceedance with id '{id}' was not found.",
            "LimitExceedance.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}