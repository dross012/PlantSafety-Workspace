// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Domain
//  File:           ComplianceLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the (self-named) Compliance aggregate's full lifecycle
//                  (Activate/Evaluate/Complete/Close/Archive), raised alongside the pre-existing
//                  ComplianceCreatedEvent. Phase 4 full build-out. Event names deliberately use
//                  the "ComplianceRule*" prefix rather than a bare "Compliance*" prefix -- this
//                  aggregate's simple name ("Compliance") collides with both its own namespace
//                  segment (PlantSafety.Domain.Compliance.Compliance) and the parent module
//                  namespace (PlantSafety.Domain.Compliance), so every disambiguation opportunity
//                  is taken. Mirrors PlantSafety.Domain.Compliance.Permits.Events.PermitsLifecycleEvents.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Compliance.Events;

/// <summary>Raised when a compliance evaluation record is activated (Created -&gt; Active).</summary>
public sealed record ComplianceRuleActivatedEvent(
    ComplianceEventId EventId,
    ComplianceId ComplianceId,
    UserId ActivatedBy,
    UtcTimestamp ActivatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IComplianceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ComplianceRuleActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ActivatedAtUtc;
}

/// <summary>Raised when an active compliance record is re-evaluated (status unchanged).</summary>
public sealed record ComplianceRuleEvaluatedEvent(
    ComplianceEventId EventId,
    ComplianceId ComplianceId,
    UserId EvaluatedBy,
    decimal ComplianceScore,
    int NonConformanceCount,
    UtcTimestamp EvaluatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IComplianceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ComplianceRuleEvaluated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => EvaluatedAtUtc;
}

/// <summary>Raised when a compliance evaluation record is marked completed (Active -&gt; Completed).</summary>
public sealed record ComplianceRuleCompletedEvent(
    ComplianceEventId EventId,
    ComplianceId ComplianceId,
    UserId CompletedBy,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IComplianceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ComplianceRuleCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when a completed compliance record is formally closed (Completed -&gt; Closed).</summary>
public sealed record ComplianceRuleClosedEvent(
    ComplianceEventId EventId,
    ComplianceId ComplianceId,
    UserId ClosedBy,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IComplianceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ComplianceRuleClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed compliance record is archived for retention (Closed -&gt; Archived).</summary>
public sealed record ComplianceRuleArchivedEvent(
    ComplianceEventId EventId,
    ComplianceId ComplianceId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IComplianceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ComplianceRuleArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
