// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPECategory
//  Layer:          Domain
//  File:           IPPECategoryFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing PPECategory aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPECategory.Interfaces;

/// <summary>Factory interface for constructing <see cref="PPECategory"/> aggregates.</summary>
public interface IPPECategoryFactory
{
    /// <summary>Creates a new PPECategory aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.PPE.PPECategory.Aggregates.PPECategory Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}