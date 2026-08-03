// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowDefinitionEvents.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain events raised by the WorkflowDefinition aggregate. Every event carries
//                  the canonical causality/correlation envelope (Handbook §2.17-2.18).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;

namespace PlantSafety.Domain.Workflow.Events;

/// <summary>Raised when a new WorkflowDefinition is created.</summary>
public sealed record WorkflowDefinitionCreatedEvent(
    WorkflowEventId EventId,
    WorkflowDefinitionId WorkflowDefinitionId,
    string WorkflowType,
    UtcTimestamp CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkflowDefinitionCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}

/// <summary>Raised when a step is added to a WorkflowDefinition.</summary>
public sealed record WorkflowStepDefinitionAddedEvent(
    WorkflowEventId EventId,
    WorkflowDefinitionId WorkflowDefinitionId,
    WorkflowStepId StepId,
    int Order,
    UtcTimestamp AddedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkflowStepDefinitionAdded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AddedAtUtc;
}

/// <summary>Raised when a WorkflowDefinition is activated and made available to start new instances.</summary>
public sealed record WorkflowDefinitionActivatedEvent(
    WorkflowEventId EventId,
    WorkflowDefinitionId WorkflowDefinitionId,
    UtcTimestamp ActivatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkflowDefinitionActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ActivatedAtUtc;
}

/// <summary>Raised when a WorkflowDefinition is deactivated, preventing new instances from starting against it.</summary>
public sealed record WorkflowDefinitionDeactivatedEvent(
    WorkflowEventId EventId,
    WorkflowDefinitionId WorkflowDefinitionId,
    UtcTimestamp DeactivatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkflowDefinitionDeactivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DeactivatedAtUtc;
}
