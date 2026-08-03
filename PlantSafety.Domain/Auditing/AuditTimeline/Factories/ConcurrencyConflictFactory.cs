// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Domain
//  File:           ConcurrencyConflictFactory.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing ConcurrencyConflictRecord
//                  aggregates. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Auditing.AuditTimeline.Aggregates;
using PlantSafety.Domain.Auditing.AuditTimeline.Interfaces;

namespace PlantSafety.Domain.Auditing.AuditTimeline.Factories;

/// <summary>Default factory implementation for constructing ConcurrencyConflictRecord aggregates.</summary>
public sealed class ConcurrencyConflictFactory : IConcurrencyConflictFactory
{
    /// <inheritdoc/>
    public ConcurrencyConflictRecord Capture(
        Guid tenantId, string entityType, Guid entityId, string expectedETag, string? actualETag,
        string attemptedOperation, Guid? actorId, UtcTimestamp occurredAtUtc,
        PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
        => ConcurrencyConflictRecord.Capture(tenantId, entityType, entityId, expectedETag, actualETag, attemptedOperation, actorId, occurredAtUtc, actor, correlationId, causationId);
}
