// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Domain
//  File:           SafetyPolicyApprovalEvents.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain events raised while a SafetyPolicy moves through its approval chain
//                  (Draft -> PendingApproval -> Approved, or back to Draft on rejection).
//                  Grounded in the Safety Policies Module's "approval workflows" and
//                  "revision history" system features.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyPolicies.ValueObjects.Ids;

namespace PlantSafety.Domain.SafetyPolicies.Events;

/// <summary>Raised when a SafetyPolicy is submitted into the approval chain.</summary>
public sealed record SafetyPolicySubmittedForApprovalEvent(
    SafetyPolicyEventId EventId,
    SafetyPolicyId      SafetyPolicyId,
    UserId              SubmittedBy,
    UtcTimestamp        SubmittedAtUtc,
    PerformedByActor    Actor,
    CorrelationId       CorrelationId,
    CausationId         CausationId
) : ISafetyPolicyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyPolicySubmittedForApproval";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => SubmittedAtUtc;
}

/// <summary>Raised when the accountable approval authority approves a SafetyPolicy revision.</summary>
public sealed record SafetyPolicyApprovedEvent(
    SafetyPolicyEventId EventId,
    SafetyPolicyId      SafetyPolicyId,
    UserId              ApprovedBy,
    UtcTimestamp        ApprovedAtUtc,
    PerformedByActor    Actor,
    CorrelationId       CorrelationId,
    CausationId         CausationId
) : ISafetyPolicyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyPolicyApproved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ApprovedAtUtc;
}

/// <summary>Raised when the approval authority rejects a SafetyPolicy revision, returning it to Draft.</summary>
public sealed record SafetyPolicyRejectedEvent(
    SafetyPolicyEventId EventId,
    SafetyPolicyId      SafetyPolicyId,
    UserId              RejectedBy,
    string              Reason,
    UtcTimestamp        RejectedAtUtc,
    PerformedByActor    Actor,
    CorrelationId       CorrelationId,
    CausationId         CausationId
) : ISafetyPolicyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyPolicyRejected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RejectedAtUtc;
}
