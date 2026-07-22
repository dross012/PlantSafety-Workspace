// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.SiteOrientation
//  Layer:          Domain
//  File:           ISiteOrientationFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing SiteOrientation aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.SiteOrientation.Interfaces;

/// <summary>Factory interface for constructing <see cref="SiteOrientation"/> aggregates.</summary>
public interface ISiteOrientationFactory
{
    /// <summary>Creates a new SiteOrientation aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.ContractorSafety.SiteOrientation.Aggregates.SiteOrientation Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}