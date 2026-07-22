// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.ErgonomicsAndMaterialHandling
//  Layer:          Domain
//  File:           IErgonomicsAndMaterialHandlingFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing ErgonomicsAndMaterialHandling aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Interfaces;

/// <summary>Factory interface for constructing <see cref="ErgonomicsAndMaterialHandling"/> aggregates.</summary>
public interface IErgonomicsAndMaterialHandlingFactory
{
    /// <summary>Creates a new ErgonomicsAndMaterialHandling aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Aggregates.ErgonomicsAndMaterialHandling Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}