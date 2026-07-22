// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditFinding
//  Layer:          Domain
//  File:           AuditFindingFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing AuditFinding aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditFinding.Factories;

/// <summary>Default factory implementation for constructing <see cref="AuditFinding"/> aggregates.</summary>
public sealed class AuditFindingFactory : IAuditFindingFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Auditing.AuditFinding.Aggregates.AuditFinding Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Auditing.AuditFinding.Aggregates.AuditFinding.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}