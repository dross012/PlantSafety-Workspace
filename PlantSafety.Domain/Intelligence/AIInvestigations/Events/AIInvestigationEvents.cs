// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Domain
//  File:           AIInvestigationEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the AIInvestigation aggregate's full lifecycle
//                  (Create/StartReview/Close/Archive). Phase 4 full build-out -- this aggregate did
//                  not exist before this pass (only a placeholder marker class existed).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.AIInvestigations.Events;

/// <summary>Raised when a new AI-assisted investigation is opened.</summary>
public sealed record AIInvestigationOpenedEvent(
    AIInvestigationEventId EventId,
    AIInvestigationId AIInvestigationId,
    Guid TenantId,
    Guid? SiteId,
    string IncidentReference,
    double AiConfidenceScore,
    UserId AssignedTo,
    UtcTimestamp OpenedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAIInvestigationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AIInvestigationOpened";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => OpenedAtUtc;
}

/// <summary>Raised when review of an AI-assisted investigation begins (Open -&gt; InProgress).</summary>
public sealed record AIInvestigationReviewStartedEvent(
    AIInvestigationEventId EventId,
    AIInvestigationId AIInvestigationId,
    UserId StartedBy,
    UtcTimestamp StartedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAIInvestigationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AIInvestigationReviewStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => StartedAtUtc;
}

/// <summary>Raised when an AI-assisted investigation is closed (InProgress -&gt; Closed).</summary>
public sealed record AIInvestigationClosedEvent(
    AIInvestigationEventId EventId,
    AIInvestigationId AIInvestigationId,
    UserId ClosedBy,
    string? ResolutionSummary,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAIInvestigationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AIInvestigationClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed AI-assisted investigation is archived for compliance retention (Closed -&gt; Archived).</summary>
public sealed record AIInvestigationArchivedEvent(
    AIInvestigationEventId EventId,
    AIInvestigationId AIInvestigationId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAIInvestigationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AIInvestigationArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
