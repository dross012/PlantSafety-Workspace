// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PermitToWork
//  Layer:          Domain
//  File:           IPermitToWorkFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing PermitToWork aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PermitToWork.Interfaces;

/// <summary>Factory interface for constructing <see cref="PermitToWork"/> aggregates.</summary>
public interface IPermitToWorkFactory
{
    /// <summary>Creates a new PermitToWork aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.ContractorSafety.PermitToWork.Aggregates.PermitToWork Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}