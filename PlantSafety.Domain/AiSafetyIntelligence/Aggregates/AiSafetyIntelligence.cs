// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Domain
//  File:           AiSafetyIntelligence.cs
//  Created:        2026-06-17
//  Updated:        2026-08-02 -- Phase 4 full build-out: extended from identity-only scaffold with
//                  TenantId/SiteId tenant-scoping, AI-generated safety insight/prediction fields
//                  (InsightType/Description/ConfidenceScore/SourceModelName), and the full
//                  Created->Active->Completed->Closed->Archived lifecycle. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing's build-out shape.
//                  Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the AiSafetyIntelligence module -- tracks a single
//                  AI-generated safety insight or prediction record.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AiSafetyIntelligence.Aggregates;

/// <summary>Aggregate root for the AiSafetyIntelligence bounded context.</summary>
public sealed class AiSafetyIntelligence
{
    /// <summary>Unique identity of this AiSafetyIntelligence aggregate.</summary>
    public AiSafetyIntelligenceId AiSafetyIntelligenceId { get; private set; }

    /// <summary>Tenant that owns this AI safety intelligence record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public AiSafetyIntelligenceStatus Status { get; private set; }

    /// <summary>Insight classification: AnomalyDetection, PredictiveRisk, PatternRecognition.</summary>
    public string InsightType { get; private set; } = "AnomalyDetection";

    /// <summary>Narrative description of the AI-generated insight.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Confidence score for the insight, in the range 0-1.</summary>
    public decimal ConfidenceScore { get; private set; }

    /// <summary>Name of the source AI/ML model that produced this insight.</summary>
    public string SourceModelName { get; private set; } = string.Empty;

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // Domain events
    private readonly List<IAiSafetyIntelligenceDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAiSafetyIntelligenceDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAiSafetyIntelligenceDomainEvent>(_domainEvents);

    private AiSafetyIntelligence() { }

    /// <summary>Creates a new AiSafetyIntelligence aggregate and raises <see cref="AiSafetyIntelligenceCreatedEvent"/>.</summary>
    /// <param name="createdBy">CRUD authorship identity.</param>
    /// <param name="createdAtUtc">Creation timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="siteId">Optional owning site.</param>
    /// <param name="insightType">Insight classification (AnomalyDetection/PredictiveRisk/PatternRecognition).</param>
    /// <param name="description">Narrative description of the insight.</param>
    /// <param name="confidenceScore">Confidence score in the range 0-1.</param>
    /// <param name="sourceModelName">Name of the source AI/ML model.</param>
    public static AiSafetyIntelligence Create(
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string insightType = "AnomalyDetection",
        string description = "",
        decimal confidenceScore = 0m,
        string sourceModelName = "")
    {
        if (confidenceScore < 0m || confidenceScore > 1m)
            throw new ArgumentOutOfRangeException(nameof(confidenceScore), "ConfidenceScore must be between 0 and 1.");

        var agg = new AiSafetyIntelligence
        {
            AiSafetyIntelligenceId  = AiSafetyIntelligenceId.New(),
            TenantId                = tenantId,
            SiteId                  = siteId,
            Status                  = AiSafetyIntelligenceStatus.Created,
            InsightType             = (insightType ?? "AnomalyDetection").Trim(),
            Description             = (description ?? string.Empty).Trim(),
            ConfidenceScore         = confidenceScore,
            SourceModelName         = (sourceModelName ?? string.Empty).Trim(),
            CreatedAtUtc            = createdAtUtc,
            CreatedBy               = createdBy,
        };

        agg._domainEvents.Add(new AiSafetyIntelligenceCreatedEvent(
            EventId:       Uuid7.New(),
            AiSafetyIntelligenceId:      agg.AiSafetyIntelligenceId.Value,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: Activate / Complete / Close / Archive -------------------

    /// <summary>Activates the AI safety intelligence record (Created -&gt; Active). Valid only from <see cref="AiSafetyIntelligenceStatus.Created"/>.</summary>
    public void Activate(
        UserId actorUserId, UtcTimestamp atUtc, PerformedByActor actor,
        CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AiSafetyIntelligenceLifecycleRules.CanActivate, AiSafetyIntelligenceStatus.Created);

        Status = AiSafetyIntelligenceStatus.Active;

        _domainEvents.Add(new AiSafetyIntelligenceActivatedEvent(
            EventId: Uuid7.New(),
            AiSafetyIntelligenceId: AiSafetyIntelligenceId.Value,
            PerformedBy: actorUserId.Value,
            OccurredAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Completes the AI safety intelligence record (Active -&gt; Completed). Valid only from <see cref="AiSafetyIntelligenceStatus.Active"/>.</summary>
    public void Complete(
        UserId actorUserId, UtcTimestamp atUtc, PerformedByActor actor,
        CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AiSafetyIntelligenceLifecycleRules.CanComplete, AiSafetyIntelligenceStatus.Active);

        Status = AiSafetyIntelligenceStatus.Completed;

        _domainEvents.Add(new AiSafetyIntelligenceCompletedEvent(
            EventId: Uuid7.New(),
            AiSafetyIntelligenceId: AiSafetyIntelligenceId.Value,
            PerformedBy: actorUserId.Value,
            OccurredAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Closes the AI safety intelligence record (Completed -&gt; Closed). Valid only from <see cref="AiSafetyIntelligenceStatus.Completed"/>.</summary>
    public void Close(
        UserId actorUserId, UtcTimestamp atUtc, PerformedByActor actor,
        CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AiSafetyIntelligenceLifecycleRules.CanClose, AiSafetyIntelligenceStatus.Completed);

        Status = AiSafetyIntelligenceStatus.Closed;

        _domainEvents.Add(new AiSafetyIntelligenceClosedEvent(
            EventId: Uuid7.New(),
            AiSafetyIntelligenceId: AiSafetyIntelligenceId.Value,
            PerformedBy: actorUserId.Value,
            OccurredAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Archives the AI safety intelligence record (Closed -&gt; Archived). Valid only from <see cref="AiSafetyIntelligenceStatus.Closed"/>.</summary>
    public void Archive(
        UserId actorUserId, UtcTimestamp atUtc, PerformedByActor actor,
        CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AiSafetyIntelligenceLifecycleRules.CanArchive, AiSafetyIntelligenceStatus.Closed);

        Status = AiSafetyIntelligenceStatus.Archived;

        _domainEvents.Add(new AiSafetyIntelligenceArchivedEvent(
            EventId: Uuid7.New(),
            AiSafetyIntelligenceId: AiSafetyIntelligenceId.Value,
            PerformedBy: actorUserId.Value,
            OccurredAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<AiSafetyIntelligenceStatus, bool> guard, AiSafetyIntelligenceStatus expected)
    {
        if (guard(Status)) return;

        throw new AiSafetyIntelligenceInvalidStatusTransitionException(
            AiSafetyIntelligenceId.Value, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
