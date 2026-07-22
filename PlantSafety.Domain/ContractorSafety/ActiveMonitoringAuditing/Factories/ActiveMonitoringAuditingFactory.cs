// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.ActiveMonitoringAuditing
//  Layer:          Domain
//  File:           ActiveMonitoringAuditingFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing ActiveMonitoringAuditing aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Factories;

/// <summary>Default factory implementation for constructing <see cref="ActiveMonitoringAuditing"/> aggregates.</summary>
public sealed class ActiveMonitoringAuditingFactory : IActiveMonitoringAuditingFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Aggregates.ActiveMonitoringAuditing Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Aggregates.ActiveMonitoringAuditing.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}