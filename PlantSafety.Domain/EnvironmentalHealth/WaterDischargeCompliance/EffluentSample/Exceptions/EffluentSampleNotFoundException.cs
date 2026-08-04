// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.EffluentSample
//  Layer:          Domain
//  File:           EffluentSampleNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a EffluentSample aggregate lookup finds no match. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Exceptions;

/// <summary>Thrown when a <see cref="EffluentSample"/> aggregate lookup finds no match.</summary>
public sealed class EffluentSampleNotFoundException : EffluentSampleDomainException
{
    /// <summary>Creates a not-found exception for the given EffluentSample identity.</summary>
    public EffluentSampleNotFoundException(EffluentSampleId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"EffluentSample with id '{id}' was not found.",
            "EffluentSample.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}