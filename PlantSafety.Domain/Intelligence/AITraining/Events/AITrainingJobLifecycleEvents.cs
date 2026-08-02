// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Domain
//  File:           AITrainingJobLifecycleEvents.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain events for the AITrainingJob aggregate's full lifecycle
//                  (Create/Start/Complete/Fail/Archive). Phase 4 full build-out -- this aggregate
//                  did not exist before this pass (only a placeholder marker class and a read-only
//                  date-range query slice existed). Mirrors
//                  PlantSafety.Domain.ContinuousImprovement.RCA.Events.RCAEvents.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.AITraining.ValueObjects.Ids;

namespace PlantSafety.Domain.Intelligence.AITraining.Events;

/// <summary>Raised when a new Intelligence.AITraining training job record is created.</summary>
public sealed record AITrainingJobCreatedEvent(
    AITrainingJobEventId EventId,
    AITrainingJobId AITrainingJobId,
    Guid TenantId,
    Guid? SiteId,
    string ModelName,
    string ModelVersion,
    string? DatasetReference,
    UserId InitiatedBy,
    UtcTimestamp StartedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAITrainingJobDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AITrainingJobCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => StartedAtUtc;
}

/// <summary>Raised when a training job begins running (Queued -&gt; Running).</summary>
public sealed record AITrainingJobStartedEvent(
    AITrainingJobEventId EventId,
    AITrainingJobId AITrainingJobId,
    UserId StartedBy,
    UtcTimestamp StartedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAITrainingJobDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AITrainingJobStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => StartedAtUtc;
}

/// <summary>Raised when a training job completes successfully (Running -&gt; Completed).</summary>
public sealed record AITrainingJobCompletedEvent(
    AITrainingJobEventId EventId,
    AITrainingJobId AITrainingJobId,
    string? ModelArtifactReference,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAITrainingJobDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AITrainingJobCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when a training job fails (Running -&gt; Failed).</summary>
public sealed record AITrainingJobFailedEvent(
    AITrainingJobEventId EventId,
    AITrainingJobId AITrainingJobId,
    string FailureReason,
    UtcTimestamp FailedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAITrainingJobDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AITrainingJobFailed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => FailedAtUtc;
}

/// <summary>Raised when a completed or failed training job is archived for compliance retention (Completed/Failed -&gt; Archived).</summary>
public sealed record AITrainingJobArchivedEvent(
    AITrainingJobEventId EventId,
    AITrainingJobId AITrainingJobId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAITrainingJobDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AITrainingJobArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
