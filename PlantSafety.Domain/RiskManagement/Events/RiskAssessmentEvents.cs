// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           RiskAssessmentEvents.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the real 3-phase Risk Management process:
//                  scoring, prioritization, control-measure application, effectiveness
//                  validation, periodic review, and closure. Grounded in "Full Risk Management
//                  Process (Detailed & Structured)" in PlantSafety_Program_Structure_extract.txt.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.RiskManagement.Enums;
using PlantSafety.Domain.RiskManagement.ValueObjects;
using PlantSafety.Domain.RiskManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.RiskManagement.Events;

/// <summary>Raised when a hazard's Severity x Likelihood risk score is recorded (Phase 1, Step 2).</summary>
public sealed record RiskScoredEvent(
    RiskEventId      EventId,
    RiskId           RiskId,
    RiskScore        Score,
    RiskLevel        Level,
    UtcTimestamp     ScoredAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IRiskDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RiskScored";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ScoredAtUtc;
}

/// <summary>Raised when a P1-P4 remediation priority is assigned (Phase 1, Step 3).</summary>
public sealed record RiskPrioritizedEvent(
    RiskEventId      EventId,
    RiskId           RiskId,
    RiskPriority     Priority,
    UtcTimestamp     PrioritizedAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IRiskDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RiskPrioritized";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => PrioritizedAtUtc;
}

/// <summary>Raised when a control measure is applied from the Hierarchy of Controls (Phase 2, Steps 5-6).</summary>
public sealed record RiskControlMeasureAppliedEvent(
    RiskEventId          EventId,
    RiskId               RiskId,
    ControlMeasureId     ControlMeasureId,
    ControlHierarchyLevel HierarchyLevel,
    UtcTimestamp         AppliedAtUtc,
    PerformedByActor     Actor,
    CorrelationId        CorrelationId,
    CausationId          CausationId
) : IRiskDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RiskControlMeasureApplied";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AppliedAtUtc;
}

/// <summary>Raised when residual risk is validated as acceptable after controls are applied (Phase 2, Step 7).</summary>
public sealed record RiskEffectivenessValidatedEvent(
    RiskEventId      EventId,
    RiskId           RiskId,
    RiskScore        ResidualScore,
    RiskLevel        ResidualLevel,
    bool             IsAcceptable,
    UtcTimestamp     ValidatedAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IRiskDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RiskEffectivenessValidated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ValidatedAtUtc;
}

/// <summary>Raised when a Mitigated risk enters its periodic monitoring/review cycle (Phase 3, Steps 9-10).</summary>
public sealed record RiskReviewCycleStartedEvent(
    RiskEventId      EventId,
    RiskId           RiskId,
    UtcTimestamp     StartedAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IRiskDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RiskReviewCycleStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => StartedAtUtc;
}

/// <summary>Raised when a periodic review completes and the risk returns to Mitigated status.</summary>
public sealed record RiskReviewCompletedEvent(
    RiskEventId      EventId,
    RiskId           RiskId,
    DateTime         NextReviewDueUtc,
    UtcTimestamp     ReviewedAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IRiskDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RiskReviewCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ReviewedAtUtc;
}

/// <summary>Raised when a Risk is formally closed.</summary>
public sealed record RiskClosedEvent(
    RiskEventId      EventId,
    RiskId           RiskId,
    UserId           ClosedBy,
    UtcTimestamp     ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IRiskDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RiskClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}
