// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Domain
//  File:           AuditingFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Auditing aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Auditing.Factories;

/// <summary>Default factory implementation for constructing <see cref="Auditing"/> aggregates.</summary>
public sealed class AuditingFactory : IAuditingFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
