// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Domain
//  File:           SafetyPolicyLifecycleEvents.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the post-approval lifecycle of a SafetyPolicy:
//                  publication, stakeholder acknowledgment, periodic review, and retirement.
//                  Grounded in the Safety Policies Module's "review cadence", "acknowledgment
//                  tracking", and "revision history" requirements.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyPolicies.ValueObjects.Ids;

namespace PlantSafety.Domain.SafetyPolicies.Events;

/// <summary>Raised when an approved SafetyPolicy becomes effective and published.</summary>
public sealed record SafetyPolicyPublishedEvent(
    SafetyPolicyEventId EventId,
    SafetyPolicyId      SafetyPolicyId,
    UtcTimestamp        EffectiveAtUtc,
    DateTime            NextReviewDueUtc,
    PerformedByActor    Actor,
    CorrelationId       CorrelationId,
    CausationId         CausationId
) : ISafetyPolicyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyPolicyPublished";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => EffectiveAtUtc;
}

/// <summary>Raised when a stakeholder acknowledges the currently published SafetyPolicy revision.</summary>
public sealed record SafetyPolicyAcknowledgedEvent(
    SafetyPolicyEventId EventId,
    SafetyPolicyId      SafetyPolicyId,
    UserId              AcknowledgedBy,
    int                 PolicyVersion,
    int                 TotalAcknowledgmentCount,
    UtcTimestamp        AcknowledgedAtUtc,
    PerformedByActor    Actor,
    CorrelationId       CorrelationId,
    CausationId         CausationId
) : ISafetyPolicyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyPolicyAcknowledged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AcknowledgedAtUtc;
}

/// <summary>Raised when a published SafetyPolicy enters its mandated periodic review cycle.</summary>
public sealed record SafetyPolicyReviewCycleStartedEvent(
    SafetyPolicyEventId EventId,
    SafetyPolicyId      SafetyPolicyId,
    UserId              StartedBy,
    UtcTimestamp        StartedAtUtc,
    PerformedByActor    Actor,
    CorrelationId       CorrelationId,
    CausationId         CausationId
) : ISafetyPolicyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyPolicyReviewCycleStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => StartedAtUtc;
}

/// <summary>Raised when a periodic review completes and the (possibly revised) policy is republished.</summary>
public sealed record SafetyPolicyReviewCompletedEvent(
    SafetyPolicyEventId EventId,
    SafetyPolicyId      SafetyPolicyId,
    UserId              ReviewedBy,
    int                 NewPolicyVersion,
    DateTime            NextReviewDueUtc,
    UtcTimestamp        ReviewedAtUtc,
    PerformedByActor    Actor,
    CorrelationId       CorrelationId,
    CausationId         CausationId
) : ISafetyPolicyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyPolicyReviewCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ReviewedAtUtc;
}

/// <summary>Raised when a SafetyPolicy is retired/withdrawn and no longer governs behavior.</summary>
public sealed record SafetyPolicyRetiredEvent(
    SafetyPolicyEventId EventId,
    SafetyPolicyId      SafetyPolicyId,
    UserId              RetiredBy,
    string              Reason,
    UtcTimestamp        RetiredAtUtc,
    PerformedByActor    Actor,
    CorrelationId       CorrelationId,
    CausationId         CausationId
) : ISafetyPolicyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyPolicyRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RetiredAtUtc;
}
