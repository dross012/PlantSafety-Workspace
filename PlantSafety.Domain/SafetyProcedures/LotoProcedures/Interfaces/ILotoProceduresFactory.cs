// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.LotoProcedures
//  Layer:          Domain
//  File:           ILotoProceduresFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing LotoProcedures aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.LotoProcedures.Interfaces;

/// <summary>Factory interface for constructing <see cref="LotoProcedures"/> aggregates.</summary>
public interface ILotoProceduresFactory
{
    /// <summary>Creates a new LotoProcedures aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.SafetyProcedures.LotoProcedures.Aggregates.LotoProcedures Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}