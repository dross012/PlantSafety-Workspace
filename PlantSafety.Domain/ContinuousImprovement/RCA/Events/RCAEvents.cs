// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Domain
//  File:           RCAEvents.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain events for the RCA aggregate's full lifecycle
//                  (Create/StartAnalysis/SubmitForApproval/Close/Archive). Phase 4 full
//                  build-out -- this aggregate did not exist before this pass (only a placeholder
//                  marker class and a read-only date-range query slice existed). Mirrors
//                  PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Events.RootCauseAnalysisEvents.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ContinuousImprovement.RCA.ValueObjects.Ids;

namespace PlantSafety.Domain.ContinuousImprovement.RCA.Events;

/// <summary>Raised when a new ContinuousImprovement RCA record is created.</summary>
public sealed record RCACreatedEvent(
    RCAEventId EventId,
    RCAId RcaId,
    Guid TenantId,
    Guid? SiteId,
    string ProblemStatement,
    string Methodology,
    UserId OwnerUserId,
    UtcTimestamp InitiatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRCADomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RCACreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => InitiatedAtUtc;
}

/// <summary>Raised when analysis begins on an RCA record (Initiated -&gt; InProgress).</summary>
public sealed record RCAAnalysisStartedEvent(
    RCAEventId EventId,
    RCAId RcaId,
    UserId StartedBy,
    UtcTimestamp StartedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRCADomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RCAAnalysisStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => StartedAtUtc;
}

/// <summary>Raised when an RCA record is submitted for approval (InProgress -&gt; PendingApproval).</summary>
public sealed record RCASubmittedForApprovalEvent(
    RCAEventId EventId,
    RCAId RcaId,
    UserId SubmittedBy,
    string? Findings,
    UtcTimestamp SubmittedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRCADomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RCASubmittedForApproval";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => SubmittedAtUtc;
}

/// <summary>Raised when an RCA record is approved and formally closed (PendingApproval -&gt; Closed).</summary>
public sealed record RCAClosedEvent(
    RCAEventId EventId,
    RCAId RcaId,
    UserId ClosedBy,
    string? RootCauseSummary,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRCADomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RCAClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed RCA record is archived for compliance retention (Closed -&gt; Archived).</summary>
public sealed record RCAArchivedEvent(
    RCAEventId EventId,
    RCAId RcaId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRCADomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RCAArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
