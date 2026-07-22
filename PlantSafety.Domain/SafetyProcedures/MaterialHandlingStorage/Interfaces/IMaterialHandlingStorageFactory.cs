// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MaterialHandlingStorage
//  Layer:          Domain
//  File:           IMaterialHandlingStorageFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing MaterialHandlingStorage aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Interfaces;

/// <summary>Factory interface for constructing <see cref="MaterialHandlingStorage"/> aggregates.</summary>
public interface IMaterialHandlingStorageFactory
{
    /// <summary>Creates a new MaterialHandlingStorage aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Aggregates.MaterialHandlingStorage Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}