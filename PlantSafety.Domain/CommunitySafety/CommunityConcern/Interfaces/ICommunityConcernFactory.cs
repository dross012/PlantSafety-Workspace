// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityConcern
//  Layer:          Domain
//  File:           ICommunityConcernFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing CommunityConcern aggregates. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Enums;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityConcern.Aggregates.CommunityConcern;

namespace PlantSafety.Domain.CommunitySafety.CommunityConcern.Interfaces;

/// <summary>Factory interface for constructing <see cref="Agg"/> aggregates.</summary>
public interface ICommunityConcernFactory
{
    /// <summary>Creates a new CommunityConcern aggregate with the supplied parameters.</summary>
    Agg Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid siteId,
        CommunityConcernCategory category,
        string severity,
        string facilityArea,
        string description,
        Guid? reportedByContactId,
        Guid? relatedIncidentId);
}
