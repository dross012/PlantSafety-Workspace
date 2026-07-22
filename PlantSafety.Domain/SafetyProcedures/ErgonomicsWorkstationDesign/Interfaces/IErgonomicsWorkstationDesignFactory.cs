// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.ErgonomicsWorkstationDesign
//  Layer:          Domain
//  File:           IErgonomicsWorkstationDesignFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing ErgonomicsWorkstationDesign aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Interfaces;

/// <summary>Factory interface for constructing <see cref="ErgonomicsWorkstationDesign"/> aggregates.</summary>
public interface IErgonomicsWorkstationDesignFactory
{
    /// <summary>Creates a new ErgonomicsWorkstationDesign aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Aggregates.ErgonomicsWorkstationDesign Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}