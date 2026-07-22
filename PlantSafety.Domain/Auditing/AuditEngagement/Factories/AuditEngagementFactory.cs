// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement
//  Layer:          Domain
//  File:           AuditEngagementFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing AuditEngagement aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditEngagement.Factories;

/// <summary>Default factory implementation for constructing <see cref="AuditEngagement"/> aggregates.</summary>
public sealed class AuditEngagementFactory : IAuditEngagementFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}