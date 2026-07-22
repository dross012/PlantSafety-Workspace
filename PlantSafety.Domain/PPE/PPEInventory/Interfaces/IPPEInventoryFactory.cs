// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEInventory
//  Layer:          Domain
//  File:           IPPEInventoryFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing PPEInventory aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEInventory.Interfaces;

/// <summary>Factory interface for constructing <see cref="PPEInventory"/> aggregates.</summary>
public interface IPPEInventoryFactory
{
    /// <summary>Creates a new PPEInventory aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.PPE.PPEInventory.Aggregates.PPEInventory Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}