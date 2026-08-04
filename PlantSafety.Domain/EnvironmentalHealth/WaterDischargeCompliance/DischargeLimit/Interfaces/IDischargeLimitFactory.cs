// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit
//  Layer:          Domain
//  File:           IDischargeLimitFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing DischargeLimit aggregates. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Interfaces;

/// <summary>Factory interface for constructing <see cref="DischargeLimit"/> aggregates.</summary>
public interface IDischargeLimitFactory
{
    /// <summary>Creates a new DischargeLimit aggregate with default field values (base audit fields only).</summary>
    PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Aggregates.DischargeLimit Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}