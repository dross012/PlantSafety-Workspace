// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePermit
//  Layer:          Domain
//  File:           IDischargePermitFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing DischargePermit aggregates. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Interfaces;

/// <summary>Factory interface for constructing <see cref="DischargePermit"/> aggregates.</summary>
public interface IDischargePermitFactory
{
    /// <summary>Creates a new DischargePermit aggregate with default field values (base audit fields only).</summary>
    PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Aggregates.DischargePermit Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}