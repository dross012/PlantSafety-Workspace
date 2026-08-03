// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Domain
//  File:           IConcurrencyConflictFactory.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Factory contract for constructing ConcurrencyConflictRecord aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Auditing.AuditTimeline.Aggregates;

namespace PlantSafety.Domain.Auditing.AuditTimeline.Interfaces;

/// <summary>Factory contract for constructing ConcurrencyConflictRecord aggregates.</summary>
public interface IConcurrencyConflictFactory
{
    /// <summary>Captures a new concurrency conflict record.</summary>
    ConcurrencyConflictRecord Capture(
        Guid tenantId, string entityType, Guid entityId, string expectedETag, string? actualETag,
        string attemptedOperation, Guid? actorId, UtcTimestamp occurredAtUtc,
        PerformedByActor actor, CorrelationId correlationId, CausationId causationId);
}
