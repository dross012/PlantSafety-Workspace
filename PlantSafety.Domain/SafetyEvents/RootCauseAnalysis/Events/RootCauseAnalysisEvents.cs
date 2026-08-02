// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Domain
//  File:           RootCauseAnalysisEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the RootCauseAnalysis aggregate's full lifecycle
//                  (Start/SubmitForReview/Approve/Close/Archive). Phase 4 full build-out -- this
//                  aggregate did not exist before this pass (only a placeholder marker class and a
//                  read-only query slice existed).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Events;

/// <summary>Raised when a new root cause analysis is started.</summary>
public sealed record RootCauseAnalysisStartedEvent(
    RootCauseAnalysisEventId EventId,
    RootCauseAnalysisId RootCauseAnalysisId,
    Guid TenantId,
    Guid? SiteId,
    Guid? RelatedEventId,
    string Methodology,
    UserId LeadAnalyst,
    UtcTimestamp StartedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRootCauseAnalysisDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RootCauseAnalysisStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => StartedAtUtc;
}

/// <summary>Raised when a root cause analysis is submitted for review (InProgress -&gt; PendingReview).</summary>
public sealed record RootCauseAnalysisSubmittedForReviewEvent(
    RootCauseAnalysisEventId EventId,
    RootCauseAnalysisId RootCauseAnalysisId,
    UserId SubmittedBy,
    UtcTimestamp SubmittedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRootCauseAnalysisDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RootCauseAnalysisSubmittedForReview";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => SubmittedAtUtc;
}

/// <summary>Raised when a root cause analysis is reviewed and approved (PendingReview -&gt; Completed).</summary>
public sealed record RootCauseAnalysisApprovedEvent(
    RootCauseAnalysisEventId EventId,
    RootCauseAnalysisId RootCauseAnalysisId,
    UserId ApprovedBy,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRootCauseAnalysisDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RootCauseAnalysisApproved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when a completed root cause analysis is formally closed (Completed -&gt; Closed).</summary>
public sealed record RootCauseAnalysisClosedEvent(
    RootCauseAnalysisEventId EventId,
    RootCauseAnalysisId RootCauseAnalysisId,
    UserId ClosedBy,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRootCauseAnalysisDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RootCauseAnalysisClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed root cause analysis is archived for compliance retention (Closed -&gt; Archived).</summary>
public sealed record RootCauseAnalysisArchivedEvent(
    RootCauseAnalysisEventId EventId,
    RootCauseAnalysisId RootCauseAnalysisId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRootCauseAnalysisDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RootCauseAnalysisArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
