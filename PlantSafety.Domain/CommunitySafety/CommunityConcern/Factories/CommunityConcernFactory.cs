// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityConcern
//  Layer:          Domain
//  File:           CommunityConcernFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing CommunityConcern aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Enums;
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Interfaces;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityConcern.Aggregates.CommunityConcern;

namespace PlantSafety.Domain.CommunitySafety.CommunityConcern.Factories;

/// <summary>Default factory implementation for constructing <see cref="Agg"/> aggregates.</summary>
public sealed class CommunityConcernFactory : ICommunityConcernFactory
{
    /// <inheritdoc/>
    public Agg Create(
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
        Guid? relatedIncidentId)
        => Agg.Log(
            createdBy, createdAtUtc, correlationId, causationId, actor,
            tenantId, siteId, category, severity, facilityArea, description,
            reportedByContactId, relatedIncidentId);
}
