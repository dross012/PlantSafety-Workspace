// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           RouteDefinitionLifecycleEvents.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events for the RouteDefinition aggregate's approval/versioning
//                  lifecycle (SubmitForReview/Approve/Retire/Archive/Revise), reused wholesale
//                  from ProcessDefinition's identical lifecycle shape. Mirrors
//                  PlantSafety.Domain.ProcessManagement.Events.ProcessDefinitionLifecycleEvents.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.ProcessManagement.Routing.Events;

/// <summary>Raised when a route definition is submitted for review (Draft -&gt; UnderReview).</summary>
public sealed record RouteDefinitionSubmittedForReviewEvent(
    Guid EventId,
    Guid RouteDefinitionId,
    Guid SubmittedByUserId,
    UtcTimestamp SubmittedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRouteDefinitionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RouteDefinitionSubmittedForReview";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => SubmittedAtUtc;
}

/// <summary>Raised when a route definition is approved and becomes effective (UnderReview -&gt; Active).</summary>
public sealed record RouteDefinitionApprovedEvent(
    Guid EventId,
    Guid RouteDefinitionId,
    Guid ApprovedByUserId,
    UtcTimestamp ApprovedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRouteDefinitionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RouteDefinitionApproved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ApprovedAtUtc;
}

/// <summary>Raised when an active route definition is retired (Active -&gt; Retired).</summary>
public sealed record RouteDefinitionRetiredEvent(
    Guid EventId,
    Guid RouteDefinitionId,
    Guid RetiredByUserId,
    string? Reason,
    UtcTimestamp RetiredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRouteDefinitionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RouteDefinitionRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RetiredAtUtc;
}

/// <summary>Raised when a retired route definition is archived for compliance retention (Retired -&gt; Archived).</summary>
public sealed record RouteDefinitionArchivedEvent(
    Guid EventId,
    Guid RouteDefinitionId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRouteDefinitionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RouteDefinitionArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}

/// <summary>
/// Raised when an active route definition is revised in place (same-status action; Active
/// remains Active) with a replaced ordered step list. Increments
/// <see cref="PlantSafety.Domain.ProcessManagement.Routing.Aggregates.RouteDefinition.Version"/>
/// and results in a new <c>RouteVersionSnapshot</c> being captured.
/// </summary>
public sealed record RouteDefinitionRevisedEvent(
    Guid EventId,
    Guid RouteDefinitionId,
    Guid RevisedByUserId,
    int Version,
    UtcTimestamp RevisedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRouteDefinitionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RouteDefinitionRevised";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RevisedAtUtc;
}
