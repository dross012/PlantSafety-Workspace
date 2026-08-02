// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.PredictiveSafety
//  Layer:          Domain
//  File:           PredictiveSafetyAlertLifecycleEvents.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain events for the PredictiveSafetyAlert aggregate's full lifecycle
//                  (Create/Acknowledge/Resolve/Archive). Phase 4 full build-out -- this aggregate
//                  did not exist before this pass (only a placeholder marker class and a read-only
//                  date-range query slice existed). Mirrors
//                  PlantSafety.Domain.ContinuousImprovement.RCA.Events.RCAEvents.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.PredictiveSafety.ValueObjects.Ids;

namespace PlantSafety.Domain.Intelligence.PredictiveSafety.Events;

/// <summary>Raised when a new Intelligence.PredictiveSafety alert record is created.</summary>
public sealed record PredictiveSafetyAlertCreatedEvent(
    PredictiveSafetyAlertEventId EventId,
    PredictiveSafetyAlertId PredictiveSafetyAlertId,
    Guid TenantId,
    Guid? SiteId,
    string RiskCategory,
    double PredictedRiskScore,
    string? ModelVersion,
    UserId GeneratedBy,
    UtcTimestamp PredictedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPredictiveSafetyAlertDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PredictiveSafetyAlertCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => PredictedAtUtc;
}

/// <summary>Raised when a predictive safety alert is acknowledged (Active -&gt; Acknowledged).</summary>
public sealed record PredictiveSafetyAlertAcknowledgedEvent(
    PredictiveSafetyAlertEventId EventId,
    PredictiveSafetyAlertId PredictiveSafetyAlertId,
    UserId AcknowledgedBy,
    UtcTimestamp AcknowledgedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPredictiveSafetyAlertDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PredictiveSafetyAlertAcknowledged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AcknowledgedAtUtc;
}

/// <summary>Raised when a predictive safety alert is resolved (Acknowledged -&gt; Resolved).</summary>
public sealed record PredictiveSafetyAlertResolvedEvent(
    PredictiveSafetyAlertEventId EventId,
    PredictiveSafetyAlertId PredictiveSafetyAlertId,
    UserId ResolvedBy,
    string? ResolutionNotes,
    UtcTimestamp ResolvedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPredictiveSafetyAlertDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PredictiveSafetyAlertResolved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ResolvedAtUtc;
}

/// <summary>Raised when a resolved predictive safety alert is archived for compliance retention (Resolved -&gt; Archived).</summary>
public sealed record PredictiveSafetyAlertArchivedEvent(
    PredictiveSafetyAlertEventId EventId,
    PredictiveSafetyAlertId PredictiveSafetyAlertId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPredictiveSafetyAlertDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PredictiveSafetyAlertArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
