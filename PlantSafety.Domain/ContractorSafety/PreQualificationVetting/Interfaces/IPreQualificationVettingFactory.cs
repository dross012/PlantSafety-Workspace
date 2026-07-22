// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PreQualificationVetting
//  Layer:          Domain
//  File:           IPreQualificationVettingFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing PreQualificationVetting aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Interfaces;

/// <summary>Factory interface for constructing <see cref="PreQualificationVetting"/> aggregates.</summary>
public interface IPreQualificationVettingFactory
{
    /// <summary>Creates a new PreQualificationVetting aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Aggregates.PreQualificationVetting Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}