// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Domain
//  File:           FinancialRecordLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the new FinancialRecord aggregate's full lifecycle
//                  (Create/AddLineItem/Submit/Approve/Reject/Close/Archive). Phase 4 full
//                  build-out -- the Financials module previously had no aggregate root at all.
//                  Mirrors PlantSafety.Domain.Compliance.Auditing.Events.AuditingLifecycleEvents.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Financials.Events;

/// <summary>Raised when a new FinancialRecord is created (Draft).</summary>
public sealed record FinancialRecordCreatedEvent(
    Guid EventId,
    Guid FinancialRecordId,
    Guid TenantId,
    UserId RecordedBy,
    UtcTimestamp RecordedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IFinancialsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "FinancialRecordCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RecordedAtUtc;
}

/// <summary>Raised when a line item is added to a Draft FinancialRecord.</summary>
public sealed record FinancialRecordLineItemAddedEvent(
    Guid EventId,
    Guid FinancialRecordId,
    Guid LineItemId,
    string Description,
    decimal Amount,
    UtcTimestamp AddedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IFinancialsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "FinancialRecordLineItemAdded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AddedAtUtc;
}

/// <summary>Raised when a FinancialRecord is submitted for approval (Draft -&gt; Submitted).</summary>
public sealed record FinancialRecordSubmittedEvent(
    Guid EventId,
    Guid FinancialRecordId,
    UserId SubmittedBy,
    UtcTimestamp SubmittedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IFinancialsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "FinancialRecordSubmitted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => SubmittedAtUtc;
}

/// <summary>Raised when a FinancialRecord is approved (Submitted -&gt; Approved).</summary>
public sealed record FinancialRecordApprovedEvent(
    Guid EventId,
    Guid FinancialRecordId,
    UserId ApprovedBy,
    UtcTimestamp ApprovedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IFinancialsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "FinancialRecordApproved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ApprovedAtUtc;
}

/// <summary>Raised when a FinancialRecord is rejected (Submitted -&gt; Rejected).</summary>
public sealed record FinancialRecordRejectedEvent(
    Guid EventId,
    Guid FinancialRecordId,
    UserId RejectedBy,
    string? Reason,
    UtcTimestamp RejectedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IFinancialsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "FinancialRecordRejected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RejectedAtUtc;
}

/// <summary>Raised when a FinancialRecord is closed (Approved -&gt; Closed).</summary>
public sealed record FinancialRecordClosedEvent(
    Guid EventId,
    Guid FinancialRecordId,
    UserId ClosedBy,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IFinancialsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "FinancialRecordClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a FinancialRecord is archived (Closed or Rejected -&gt; Archived).</summary>
public sealed record FinancialRecordArchivedEvent(
    Guid EventId,
    Guid FinancialRecordId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IFinancialsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "FinancialRecordArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
