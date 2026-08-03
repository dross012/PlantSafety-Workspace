// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowStepDecisionEvents.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain events raised when a workflow step is resolved, whether by a human
//                  actor, an automated decision, or a surfaced predictive recommendation (Phase
//                  5.3 human + machine collaboration). Every event carries the canonical
//                  causality/correlation envelope (Handbook §2.17-2.18).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;

namespace PlantSafety.Domain.Workflow.Events;

/// <summary>Raised when a human actor approves a HumanApproval step.</summary>
public sealed record WorkflowApprovedEvent(
    WorkflowEventId EventId,
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowStepId StepId,
    UserId ApprovedBy,
    string? Notes,
    UtcTimestamp ApprovedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkflowApproved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ApprovedAtUtc;
}

/// <summary>Raised when an AutomatedDecision step is resolved by a domain policy/rule without human input.</summary>
public sealed record WorkflowAutomatedDecisionMadeEvent(
    WorkflowEventId EventId,
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowStepId StepId,
    bool Approved,
    string? Notes,
    UtcTimestamp DecidedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkflowAutomatedDecisionMade";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DecidedAtUtc;
}

/// <summary>
/// Raised when a PredictiveRecommendation step surfaces a recommendation sourced (indirectly, via
/// a SharedKernel/integration-event contract) from a predictive-analytics module such as
/// Intelligence.PredictiveSafety or Analytics.PredictiveAnalytics.
/// </summary>
public sealed record WorkflowRecommendationSurfacedEvent(
    WorkflowEventId EventId,
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowStepId StepId,
    string RecommendationReference,
    UtcTimestamp SurfacedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkflowRecommendationSurfaced";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => SurfacedAtUtc;
}
