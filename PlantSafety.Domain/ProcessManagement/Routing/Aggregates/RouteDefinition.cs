// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           RouteDefinition.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the ProcessManagement.Routing submodule. Represents a
//                  documented, versioned, ordered sequence of RouteStep entities (a manufacturing/
//                  operational "routing"). Reuses ProcessDefinition's approval/versioning
//                  lifecycle wholesale (Draft -> UnderReview -> Active -> Retired -> Archived,
//                  plus a same-status Revise action) -- see
//                  PlantSafety.Domain.ProcessManagement.Aggregates.ProcessDefinition and
//                  docs/Phase5/ui-gaps-scoping.md §9. The ordered RouteStep collection, step-level
//                  versioning via RouteVersionSnapshot, diffing (RouteDiffEngine), and simulation
//                  (RouteSimulationEngine) are genuinely new -- ProcessDefinition has no structural
//                  content concept at all. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ProcessManagement.Routing.Entities;
using PlantSafety.Domain.ProcessManagement.Routing.Enums;
using PlantSafety.Domain.ProcessManagement.Routing.Events;
using PlantSafety.Domain.ProcessManagement.Routing.Exceptions;
using PlantSafety.Domain.ProcessManagement.Routing.Rules;
using PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.ProcessManagement.Routing.Aggregates;

/// <summary>
/// Aggregate root for the ProcessManagement.Routing submodule. Tracks a single versioned,
/// ordered route (sequence of <see cref="RouteStep"/>), its workflow status, and owner.
/// </summary>
public sealed class RouteDefinition
{
    /// <summary>Unique identity of this RouteDefinition aggregate.</summary>
    public RouteDefinitionId RouteDefinitionId { get; private set; }

    /// <summary>Tenant that owns this route definition.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site/plant this route is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Human-readable plant/site display name.</summary>
    public string PlantName { get; private set; } = string.Empty;

    /// <summary>Human-readable route name (e.g., "Tank Farm B Fill Sequence").</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Current lifecycle status.</summary>
    public RouteDefinitionStatus Status { get; private set; } = RouteDefinitionStatus.Draft;

    /// <summary>Monotonically increasing revision number, starting at 1.</summary>
    public int Version { get; private set; } = 1;

    /// <summary>Identity of the route owner (accountable role holder).</summary>
    public UserId OwnerId { get; private set; }

    /// <summary>Display name of the route owner (denormalized for read-model convenience).</summary>
    public string OwnerName { get; private set; } = string.Empty;

    /// <summary>Effective start date (UTC) for this route.</summary>
    public UtcTimestamp EffectiveFromUtc { get; private set; }

    /// <summary>Effective end date (UTC), if any.</summary>
    public UtcTimestamp? EffectiveToUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>UTC timestamp of the last lifecycle/audit-relevant event on this aggregate.</summary>
    public UtcTimestamp LastAuditedAtUtc { get; private set; }

    private readonly List<RouteStep> _steps = [];

    /// <summary>The route's current ordered step sequence.</summary>
    public IReadOnlyList<RouteStep> Steps => new ReadOnlyCollection<RouteStep>(_steps);

    private readonly List<IRouteDefinitionDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IRouteDefinitionDomainEvent> DomainEvents
        => new ReadOnlyCollection<IRouteDefinitionDomainEvent>(_domainEvents);

    private RouteDefinition() { }

    /// <summary>Creates a new RouteDefinition aggregate in Draft status with its initial ordered step list.</summary>
    public static RouteDefinition Create(
        Guid tenantId, Guid? siteId, string plantName, string name,
        IReadOnlyList<(string StepName, string Instructions, IReadOnlyList<string> Resources, int EstimatedDurationMinutes)> steps,
        UserId ownerId, string ownerName,
        UtcTimestamp effectiveFromUtc, UtcTimestamp? effectiveToUtc,
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must not be null or whitespace.", nameof(name));
        if (string.IsNullOrWhiteSpace(plantName))
            throw new ArgumentException("PlantName must not be null or whitespace.", nameof(plantName));

        var agg = new RouteDefinition
        {
            RouteDefinitionId = RouteDefinitionId.New(),
            TenantId          = tenantId,
            SiteId            = siteId,
            PlantName         = plantName.Trim(),
            Name              = name.Trim(),
            Status            = RouteDefinitionStatus.Draft,
            Version           = 1,
            OwnerId           = ownerId,
            OwnerName         = (ownerName ?? string.Empty).Trim(),
            EffectiveFromUtc  = effectiveFromUtc,
            EffectiveToUtc    = effectiveToUtc,
            CreatedAtUtc      = createdAtUtc,
            CreatedBy         = createdBy,
            LastAuditedAtUtc  = createdAtUtc,
        };

        agg.ReplaceSteps(steps, agg.RouteDefinitionId, correlationId, causationId);

        agg._domainEvents.Add(new RouteDefinitionCreatedEvent(
            EventId:             Uuid7.New(),
            RouteDefinitionId:   agg.RouteDefinitionId.Value,
            TenantId:            tenantId,
            Name:                agg.Name,
            CreatedAtUtc:        createdAtUtc,
            Actor:               actor,
            CorrelationId:       correlationId,
            CausationId:         causationId));

        return agg;
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>Returns an immutable snapshot of the route's current steps, for RouteVersionSnapshot capture.</summary>
    public IReadOnlyList<RouteStepSnapshot> ToStepSnapshots() => _steps.Select(s => s.ToSnapshot()).ToList();

    // ---- Lifecycle: SubmitForReview / Approve / Retire / Archive / Revise (reused from
    // ProcessDefinition's identical shape) ------------------------------------------------

    /// <summary>Submits this Draft route definition for review (Draft -&gt; UnderReview).</summary>
    public void SubmitForReview(UserId submittedBy, UtcTimestamp submittedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, RouteDefinitionLifecycleRules.CanSubmitForReview, RouteDefinitionStatus.Draft);

        Status = RouteDefinitionStatus.UnderReview;
        LastAuditedAtUtc = submittedAtUtc;

        AddDomainEvent(new RouteDefinitionSubmittedForReviewEvent(
            Uuid7.New(), RouteDefinitionId.Value, submittedBy.Value, submittedAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Approves a route definition under review and makes it effective (UnderReview -&gt; Active).</summary>
    public void Approve(UserId approvedBy, UtcTimestamp approvedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, RouteDefinitionLifecycleRules.CanApprove, RouteDefinitionStatus.UnderReview);

        Status = RouteDefinitionStatus.Active;
        LastAuditedAtUtc = approvedAtUtc;

        AddDomainEvent(new RouteDefinitionApprovedEvent(
            Uuid7.New(), RouteDefinitionId.Value, approvedBy.Value, approvedAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Retires an active route definition from use (Active -&gt; Retired).</summary>
    public void Retire(UserId retiredBy, string? reason, UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, RouteDefinitionLifecycleRules.CanRetire, RouteDefinitionStatus.Active);

        Status = RouteDefinitionStatus.Retired;
        LastAuditedAtUtc = retiredAtUtc;

        AddDomainEvent(new RouteDefinitionRetiredEvent(
            Uuid7.New(), RouteDefinitionId.Value, retiredBy.Value, reason, retiredAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Archives a retired route definition for compliance retention (Retired -&gt; Archived).</summary>
    public void Archive(UtcTimestamp archivedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, RouteDefinitionLifecycleRules.CanArchive, RouteDefinitionStatus.Retired);

        Status = RouteDefinitionStatus.Archived;
        LastAuditedAtUtc = archivedAtUtc;

        AddDomainEvent(new RouteDefinitionArchivedEvent(
            Uuid7.New(), RouteDefinitionId.Value, archivedAtUtc, actor, correlationId, causationId));
    }

    /// <summary>
    /// Revises an active route definition in place (same-status action; Active remains Active),
    /// replacing its ordered step list and incrementing <see cref="Version"/>. Steps whose
    /// <c>ExistingStepId</c> matches a current step preserve that <see cref="RouteStep.StepId"/>
    /// (enabling <c>RouteDiffEngine</c> to recognise in-place modifications rather than a
    /// remove+add pair); steps with no match are treated as additions.
    /// </summary>
    public void Revise(
        IReadOnlyList<(Guid? ExistingStepId, string StepName, string Instructions, IReadOnlyList<string> Resources, int EstimatedDurationMinutes)> newSteps,
        UserId revisedBy, UtcTimestamp revisedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, RouteDefinitionLifecycleRules.CanRevise, RouteDefinitionStatus.Active);

        var resolved = newSteps
            .Select(s => (s.StepName, s.Instructions, s.Resources, s.EstimatedDurationMinutes, ExistingStepId: s.ExistingStepId))
            .ToList();

        ReplaceStepsPreservingIds(resolved, correlationId, causationId);

        Version++;
        LastAuditedAtUtc = revisedAtUtc;

        AddDomainEvent(new RouteDefinitionRevisedEvent(
            Uuid7.New(), RouteDefinitionId.Value, revisedBy.Value, Version, revisedAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -------------------------------------------------------------

    private void ReplaceSteps(
        IReadOnlyList<(string StepName, string Instructions, IReadOnlyList<string> Resources, int EstimatedDurationMinutes)> steps,
        RouteDefinitionId id, CorrelationId correlationId, CausationId causationId)
    {
        if (steps is null || steps.Count == 0)
            throw new RouteDefinitionValidationException(id, "A route definition must have at least one step.", correlationId, causationId);

        _steps.Clear();
        var number = 1;
        foreach (var s in steps)
        {
            _steps.Add(RouteStep.Create(number, s.StepName, s.Instructions, s.Resources, s.EstimatedDurationMinutes));
            number++;
        }
    }

    private void ReplaceStepsPreservingIds(
        IReadOnlyList<(string StepName, string Instructions, IReadOnlyList<string> Resources, int EstimatedDurationMinutes, Guid? ExistingStepId)> steps,
        CorrelationId correlationId, CausationId causationId)
    {
        if (steps is null || steps.Count == 0)
            throw new RouteDefinitionValidationException(RouteDefinitionId, "A route definition must have at least one step.", correlationId, causationId);

        _steps.Clear();
        var number = 1;
        foreach (var s in steps)
        {
            _steps.Add(RouteStep.Rehydrate(s.ExistingStepId ?? Guid.NewGuid(), number, s.StepName, s.Instructions, s.Resources, s.EstimatedDurationMinutes));
            number++;
        }
    }

    private void AddDomainEvent(IRouteDefinitionDomainEvent evt) => _domainEvents.Add(evt);

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<RouteDefinitionStatus, bool> guard, RouteDefinitionStatus expected)
    {
        if (guard(Status)) return;

        throw new RouteDefinitionInvalidStatusTransitionException(RouteDefinitionId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
