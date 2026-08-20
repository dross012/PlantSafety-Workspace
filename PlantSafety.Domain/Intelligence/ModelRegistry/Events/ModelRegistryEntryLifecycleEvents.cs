// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Domain
//  File:           ModelRegistryEntryLifecycleEvents.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain events for the ModelRegistryEntry aggregate's full lifecycle
//                  (Register/SubmitForApproval/Approve/Deprecate/Retire). Ported (rebuilt) from the
//                  orphaned, dead
//                  Intelligence.Domain/ModelRegistry/Events/{ModelRegisteredEvent,ModelApprovedEvent,
//                  ModelDeprecatedEvent}.cs records, adding SubmittedForApproval/Retired events for
//                  full lifecycle symmetry with sibling modules, and folding in EventId/Actor/
//                  Correlation/Causation envelope fields per this module's own convention. Mirrors
//                  PlantSafety.Domain.Intelligence.DataLake.Events.DataLakeActivityLifecycleEvents.
//                  ApprovedByActorId must always be a human actor -- never a system identity
//                  (Safety Rule 4.5). DeprecationReason must contain no PII or infrastructure
//                  details.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.ModelRegistry.ValueObjects.Ids;

namespace PlantSafety.Domain.Intelligence.ModelRegistry.Events;

/// <summary>Raised when a new model version is registered in the model registry.</summary>
public sealed record ModelRegistryEntryRegisteredEvent(
    ModelRegistryEntryEventId EventId,
    ModelRegistryEntryId ModelRegistryEntryId,
    Guid TenantId,
    Guid? SiteId,
    string ModelName,
    string ModelVersion,
    string DatasetHash,
    double TrainingAccuracyScore,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IModelRegistryEntryDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ModelRegistryEntryRegistered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc { get; init; } = UtcTimestamp.Now();
}

/// <summary>Raised when a registered model is submitted for human approval (Draft -&gt; PendingApproval).</summary>
public sealed record ModelRegistryEntrySubmittedForApprovalEvent(
    ModelRegistryEntryEventId EventId,
    ModelRegistryEntryId ModelRegistryEntryId,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IModelRegistryEntryDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ModelRegistryEntrySubmittedForApproval";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc { get; init; } = UtcTimestamp.Now();
}

/// <summary>Raised when a human reviewer approves a model for production deployment (PendingApproval -&gt; Approved). Safety Rule 4.5.</summary>
public sealed record ModelRegistryEntryApprovedEvent(
    ModelRegistryEntryEventId EventId,
    ModelRegistryEntryId ModelRegistryEntryId,
    string ModelName,
    string ModelVersion,
    UserId ApprovedByActorId,
    UtcTimestamp ApprovedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IModelRegistryEntryDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ModelRegistryEntryApproved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ApprovedAtUtc;
}

/// <summary>Raised when an approved model is deprecated, typically when superseded by a newer version (Approved -&gt; Deprecated).</summary>
public sealed record ModelRegistryEntryDeprecatedEvent(
    ModelRegistryEntryEventId EventId,
    ModelRegistryEntryId ModelRegistryEntryId,
    string ModelName,
    string ModelVersion,
    string DeprecationReason,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IModelRegistryEntryDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ModelRegistryEntryDeprecated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc { get; init; } = UtcTimestamp.Now();
}

/// <summary>Raised when a model registry entry is permanently retired (terminal; no further transitions).</summary>
public sealed record ModelRegistryEntryRetiredEvent(
    ModelRegistryEntryEventId EventId,
    ModelRegistryEntryId ModelRegistryEntryId,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IModelRegistryEntryDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ModelRegistryEntryRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc { get; init; } = UtcTimestamp.Now();
}
