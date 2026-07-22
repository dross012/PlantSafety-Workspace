// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEZoneMapping
//  Layer:          Domain
//  File:           IPPEZoneMappingFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing PPEZoneMapping aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEZoneMapping.Interfaces;

/// <summary>Factory interface for constructing <see cref="PPEZoneMapping"/> aggregates.</summary>
public interface IPPEZoneMappingFactory
{
    /// <summary>Creates a new PPEZoneMapping aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates.PPEZoneMapping Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}