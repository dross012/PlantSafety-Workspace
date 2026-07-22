// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.PpePolicies
//  Layer:          Domain
//  File:           PpePoliciesCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real PPE policy document lifecycle: drafting,
//                  submission, publication, enforcement audit, revision, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.PpePolicies.Events;

/// <summary>Raised when a new PPE policy is drafted.</summary>
public sealed record PpePolicyDraftedEvent(
    PpePoliciesEventId EventId,
    PpePoliciesId PpePoliciesId,
    PolicyPpeCategory PpeCategory,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPpePoliciesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PpePolicyDrafted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the policy is submitted for approval.</summary>
public sealed record PpePolicySubmittedForApprovalEvent(
    PpePoliciesEventId EventId,
    PpePoliciesId PpePoliciesId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPpePoliciesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PpePolicySubmittedForApproval";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the policy is approved and published.</summary>
public sealed record PpePolicyPublishedEvent(
    PpePoliciesEventId EventId,
    PpePoliciesId PpePoliciesId,
    UserId ApprovedByUserId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPpePoliciesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PpePolicyPublished";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when an enforcement/compliance audit is recorded.</summary>
public sealed record PpePolicyEnforcementAuditRecordedEvent(
    PpePoliciesEventId EventId,
    PpePoliciesId PpePoliciesId,
    decimal ComplianceScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPpePoliciesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PpePolicyEnforcementAuditRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a revision cycle is opened.</summary>
public sealed record PpePolicyRevisionScheduledEvent(
    PpePoliciesEventId EventId,
    PpePoliciesId PpePoliciesId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPpePoliciesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PpePolicyRevisionScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the policy document is retired.</summary>
public sealed record PpePolicyRetiredEvent(
    PpePoliciesEventId EventId,
    PpePoliciesId PpePoliciesId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPpePoliciesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PpePolicyRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
