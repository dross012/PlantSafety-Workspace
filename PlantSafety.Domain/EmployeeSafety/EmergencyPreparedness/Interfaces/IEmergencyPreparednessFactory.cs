// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EmergencyPreparedness
//  Layer:          Domain
//  File:           IEmergencyPreparednessFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing EmergencyPreparedness aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Interfaces;

/// <summary>Factory interface for constructing <see cref="EmergencyPreparedness"/> aggregates.</summary>
public interface IEmergencyPreparednessFactory
{
    /// <summary>Creates a new EmergencyPreparedness aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Aggregates.EmergencyPreparedness Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}