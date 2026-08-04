// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.EffluentSample
//  Layer:          Domain
//  File:           IEffluentSampleFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing EffluentSample aggregates. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Interfaces;

/// <summary>Factory interface for constructing <see cref="EffluentSample"/> aggregates.</summary>
public interface IEffluentSampleFactory
{
    /// <summary>Creates a new EffluentSample aggregate with default field values (base audit fields only).</summary>
    PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Aggregates.EffluentSample Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}