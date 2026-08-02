// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Domain
//  File:           AIInvestigation.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Intelligence.AIInvestigations bounded context. Tracks a
//                  single AI-assisted safety investigation from Open through review (InProgress),
//                  Closure, and Archival. Phase 4 full build-out: this bounded context previously
//                  had no aggregate at all -- only a placeholder marker class
//                  (AIInvestigationsModuleMarker) existed. Field shape (IncidentReference,
//                  AiConfidenceScore, Summary, AssignedTo, OpenedAtUtc) is grounded in the
//                  pre-existing read-only AllInvestigationsItemDto's documented columns. Mirrors
//                  PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Aggregates
//                  .RootCauseAnalysis's guarded-transition/domain-event/exception conventions.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Intelligence.AIInvestigations.Enums;
using PlantSafety.Domain.Intelligence.AIInvestigations.Events;
using PlantSafety.Domain.Intelligence.AIInvestigations.Exceptions;
using PlantSafety.Domain.Intelligence.AIInvestigations.Rules;
using PlantSafety.Domain.Intelligence.AIInvestigations.ValueObjects.Ids;

namespace PlantSafety.Domain.Intelligence.AIInvestigations.Aggregates;

/// <summary>Aggregate root for the Intelligence.AIInvestigations bounded context.</summary>
public sealed class AIInvestigation
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this AIInvestigation aggregate.</summary>
    public AIInvestigationId AIInvestigationId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public AIInvestigationStatus Status { get; private set; }

    // ---- Investigation-specific business properties ---------------------------

    /// <summary>Reference identifier of the related safety incident.</summary>
    public string IncidentReference { get; private set; } = string.Empty;

    /// <summary>AI model confidence score (0.0-1.0) for the investigation findings.</summary>
    public double AiConfidenceScore { get; private set; }

    /// <summary>AI-generated summary of the investigation.</summary>
    public string? Summary { get; private set; }

    /// <summary>Identity of the user or process assigned to the investigation.</summary>
    public UserId AssignedTo { get; private set; }

    /// <summary>UTC timestamp when the investigation was opened.</summary>
    public UtcTimestamp OpenedAtUtc { get; private set; }

    // ---- Lifecycle attributes (StartReview/Close/Archive) ---------------------

    /// <summary>Identity of the user who started the review, if applicable.</summary>
    public UserId? ReviewStartedBy { get; private set; }

    /// <summary>UTC instant review of the investigation started.</summary>
    public UtcTimestamp? ReviewStartedAtUtc { get; private set; }

    /// <summary>Identity of the user who closed the investigation, if applicable.</summary>
    public UserId? ClosedBy { get; private set; }

    /// <summary>Resolution summary supplied when closing the investigation.</summary>
    public string? ResolutionSummary { get; private set; }

    /// <summary>UTC instant the investigation was closed.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    /// <summary>UTC instant the investigation was archived for compliance retention.</summary>
    public UtcTimestamp? ArchivedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user/system that created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IAIInvestigationDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAIInvestigationDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAIInvestigationDomainEvent>(_domainEvents);

    // For rehydration only.
    private AIInvestigation() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Opens a new AIInvestigation and records <see cref="AIInvestigationOpenedEvent"/>.</summary>
    public static AIInvestigation Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string incidentReference,
        double aiConfidenceScore,
        string? summary,
        UserId assignedTo)
    {
        if (string.IsNullOrWhiteSpace(incidentReference))
            throw new ArgumentException("IncidentReference must be supplied.", nameof(incidentReference));
        if (aiConfidenceScore is < 0.0 or > 1.0)
            throw new ArgumentOutOfRangeException(nameof(aiConfidenceScore), "AiConfidenceScore must be between 0.0 and 1.0.");

        var agg = new AIInvestigation
        {
            AIInvestigationId = AIInvestigationId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = AIInvestigationStatus.Open,
            IncidentReference = incidentReference.Trim(),
            AiConfidenceScore = aiConfidenceScore,
            Summary = summary,
            AssignedTo = assignedTo,
            OpenedAtUtc = createdAtUtc,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new AIInvestigationOpenedEvent(
            EventId: AIInvestigationEventId.New(),
            AIInvestigationId: agg.AIInvestigationId,
            TenantId: tenantId,
            SiteId: siteId,
            IncidentReference: agg.IncidentReference,
            AiConfidenceScore: aiConfidenceScore,
            AssignedTo: assignedTo,
            OpenedAtUtc: createdAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return agg;
    }

    // ---- Lifecycle: StartReview -------------------------------------------

    /// <summary>
    /// Starts review of the investigation (Open -&gt; InProgress). Valid only from
    /// <see cref="AIInvestigationStatus.Open"/>.
    /// </summary>
    public void StartReview(
        UserId startedBy,
        UtcTimestamp startedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AIInvestigationLifecycleRules.CanStartReview, AIInvestigationStatus.InProgress);

        ReviewStartedBy = startedBy;
        ReviewStartedAtUtc = startedAtUtc;
        Status = AIInvestigationStatus.InProgress;

        AddDomainEvent(new AIInvestigationReviewStartedEvent(
            EventId: AIInvestigationEventId.New(),
            AIInvestigationId: AIInvestigationId,
            StartedBy: startedBy,
            StartedAtUtc: startedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Close ------------------------------------------------------

    /// <summary>
    /// Closes the investigation (InProgress -&gt; Closed). Valid only from
    /// <see cref="AIInvestigationStatus.InProgress"/>.
    /// </summary>
    public void Close(
        UserId closedBy,
        string? resolutionSummary,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AIInvestigationLifecycleRules.CanClose, AIInvestigationStatus.Closed);

        ClosedBy = closedBy;
        ResolutionSummary = resolutionSummary;
        ClosedAtUtc = closedAtUtc;
        Status = AIInvestigationStatus.Closed;

        AddDomainEvent(new AIInvestigationClosedEvent(
            EventId: AIInvestigationEventId.New(),
            AIInvestigationId: AIInvestigationId,
            ClosedBy: closedBy,
            ResolutionSummary: resolutionSummary,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Archive -----------------------------------------------------

    /// <summary>
    /// Archives a closed investigation for compliance retention (Closed -&gt; Archived). Valid
    /// only from <see cref="AIInvestigationStatus.Closed"/>.
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AIInvestigationLifecycleRules.CanArchive, AIInvestigationStatus.Archived);

        ArchivedAtUtc = archivedAtUtc;
        Status = AIInvestigationStatus.Archived;

        AddDomainEvent(new AIInvestigationArchivedEvent(
            EventId: AIInvestigationEventId.New(),
            AIInvestigationId: AIInvestigationId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IAIInvestigationDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<AIInvestigationStatus, bool> guard, AIInvestigationStatus expected)
    {
        if (guard(Status)) return;

        throw new AIInvestigationInvalidStatusTransitionException(
            AIInvestigationId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
