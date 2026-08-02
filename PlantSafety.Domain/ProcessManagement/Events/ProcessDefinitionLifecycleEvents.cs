// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Domain
//  File:           ProcessDefinitionLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the ProcessDefinition aggregate's full lifecycle
//                  (SubmitForReview/Approve/Retire/Archive/Revise), raised alongside the
//                  pre-existing ProcessDefinitionCreatedEvent. Phase 4 full build-out -- these
//                  transitions previously did not exist on the aggregate at all.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ProcessManagement.Events;

/// <summary>Raised when a process definition is submitted for review (Draft -&gt; UnderReview).</summary>
public sealed record ProcessDefinitionSubmittedForReviewEvent(
    Guid EventId,
    Guid ProcessDefinitionId,
    Guid SubmittedByUserId,
    UtcTimestamp SubmittedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IProcessDefinitionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProcessDefinitionSubmittedForReview";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => SubmittedAtUtc;
}

/// <summary>Raised when a process definition is approved and becomes effective (UnderReview -&gt; Active).</summary>
public sealed record ProcessDefinitionApprovedEvent(
    Guid EventId,
    Guid ProcessDefinitionId,
    Guid ApprovedByUserId,
    UtcTimestamp ApprovedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IProcessDefinitionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProcessDefinitionApproved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ApprovedAtUtc;
}

/// <summary>Raised when an active process definition is retired (Active -&gt; Retired).</summary>
public sealed record ProcessDefinitionRetiredEvent(
    Guid EventId,
    Guid ProcessDefinitionId,
    Guid RetiredByUserId,
    string? Reason,
    UtcTimestamp RetiredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IProcessDefinitionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProcessDefinitionRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RetiredAtUtc;
}

/// <summary>Raised when a retired process definition is archived for compliance retention (Retired -&gt; Archived).</summary>
public sealed record ProcessDefinitionArchivedEvent(
    Guid EventId,
    Guid ProcessDefinitionId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IProcessDefinitionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProcessDefinitionArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}

/// <summary>
/// Raised when an active process definition is revised in place (same-status action; Active
/// remains Active). Increments <see cref="PlantSafety.Domain.ProcessManagement.Aggregates.ProcessDefinition.Version"/>.
/// </summary>
public sealed record ProcessDefinitionRevisedEvent(
    Guid EventId,
    Guid ProcessDefinitionId,
    Guid RevisedByUserId,
    int Version,
    UtcTimestamp RevisedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IProcessDefinitionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProcessDefinitionRevised";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RevisedAtUtc;
}
