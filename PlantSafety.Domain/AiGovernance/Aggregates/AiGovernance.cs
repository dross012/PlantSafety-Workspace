// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Domain
//  File:           AiGovernance.cs
//  Created:        2026-06-17
//  Updated:        2026-08-02 -- Phase 4 full build-out: extended from identity-only scaffold
//                  with TenantId/SiteId tenant-scoping, AI governance/oversight registry fields
//                  (SystemName/AiUseCase/RiskClassification/OwnerUserId/RequiresHumanOversight),
//                  and the full Created->Active->Completed->Closed->Archived lifecycle. This is a
//                  distinct concept from the pre-existing, unrelated "GovernedModel" Features
//                  vertical (PlantSafety.Application.Features.AiGovernance) -- not touched here.
//                  Mirrors PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing's build-out
//                  shape. Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the AiGovernance module -- tracks a single AI system's
//                  governance/oversight registry record: its use case, risk classification,
//                  accountable owner, and human-oversight requirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AiGovernance.Aggregates;

/// <summary>Aggregate root for the AiGovernance bounded context.</summary>
public sealed class AiGovernance
{
    /// <summary>Unique identity of this AiGovernance aggregate.</summary>
    public AiGovernanceId AiGovernanceId { get; private set; }

    /// <summary>Tenant that owns this AI governance record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public AiGovernanceStatus Status { get; private set; }

    /// <summary>Name of the governed AI system.</summary>
    public string SystemName { get; private set; } = string.Empty;

    /// <summary>Narrative description of the AI system's use case.</summary>
    public string AiUseCase { get; private set; } = string.Empty;

    /// <summary>Risk classification: Low, Medium, High, Critical.</summary>
    public string RiskClassification { get; private set; } = "Low";

    /// <summary>Identity of the accountable owner for this AI system.</summary>
    public UserId OwnerUserId { get; private set; }

    /// <summary>Whether this AI system requires human oversight before autonomous action.</summary>
    public bool RequiresHumanOversight { get; private set; } = true;

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // Domain events
    private readonly List<IAiGovernanceDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAiGovernanceDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAiGovernanceDomainEvent>(_domainEvents);

    private AiGovernance() { }

    /// <summary>Creates a new AiGovernance aggregate and raises <see cref="AiGovernanceCreatedEvent"/>.</summary>
    /// <param name="createdBy">CRUD authorship identity.</param>
    /// <param name="createdAtUtc">Creation timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="siteId">Optional owning site.</param>
    /// <param name="systemName">Name of the governed AI system.</param>
    /// <param name="aiUseCase">Narrative description of the AI system's use case.</param>
    /// <param name="riskClassification">Risk classification (Low/Medium/High/Critical).</param>
    /// <param name="ownerUserId">Identity of the accountable owner.</param>
    /// <param name="requiresHumanOversight">Whether human oversight is required. Defaults to <see langword="true"/>.</param>
    public static AiGovernance Create(
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string systemName = "",
        string aiUseCase = "",
        string riskClassification = "Low",
        UserId ownerUserId = default,
        bool requiresHumanOversight = true)
    {
        var agg = new AiGovernance
        {
            AiGovernanceId          = AiGovernanceId.New(),
            TenantId                = tenantId,
            SiteId                  = siteId,
            Status                  = AiGovernanceStatus.Created,
            SystemName              = (systemName ?? string.Empty).Trim(),
            AiUseCase               = (aiUseCase ?? string.Empty).Trim(),
            RiskClassification      = (riskClassification ?? "Low").Trim(),
            OwnerUserId             = ownerUserId,
            RequiresHumanOversight  = requiresHumanOversight,
            CreatedAtUtc            = createdAtUtc,
            CreatedBy               = createdBy,
        };

        agg._domainEvents.Add(new AiGovernanceCreatedEvent(
            EventId:       Uuid7.New(),
            AiGovernanceId:      agg.AiGovernanceId.Value,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: Activate / Complete / Close / Archive -------------------

    /// <summary>Activates the AI governance record (Created -&gt; Active). Valid only from <see cref="AiGovernanceStatus.Created"/>.</summary>
    public void Activate(
        UserId actorUserId, UtcTimestamp atUtc, PerformedByActor actor,
        CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AiGovernanceLifecycleRules.CanActivate, AiGovernanceStatus.Created);

        Status = AiGovernanceStatus.Active;

        _domainEvents.Add(new AiGovernanceActivatedEvent(
            EventId: Uuid7.New(),
            AiGovernanceId: AiGovernanceId.Value,
            PerformedBy: actorUserId.Value,
            OccurredAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Completes the AI governance record (Active -&gt; Completed). Valid only from <see cref="AiGovernanceStatus.Active"/>.</summary>
    public void Complete(
        UserId actorUserId, UtcTimestamp atUtc, PerformedByActor actor,
        CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AiGovernanceLifecycleRules.CanComplete, AiGovernanceStatus.Active);

        Status = AiGovernanceStatus.Completed;

        _domainEvents.Add(new AiGovernanceCompletedEvent(
            EventId: Uuid7.New(),
            AiGovernanceId: AiGovernanceId.Value,
            PerformedBy: actorUserId.Value,
            OccurredAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Closes the AI governance record (Completed -&gt; Closed). Valid only from <see cref="AiGovernanceStatus.Completed"/>.</summary>
    public void Close(
        UserId actorUserId, UtcTimestamp atUtc, PerformedByActor actor,
        CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AiGovernanceLifecycleRules.CanClose, AiGovernanceStatus.Completed);

        Status = AiGovernanceStatus.Closed;

        _domainEvents.Add(new AiGovernanceClosedEvent(
            EventId: Uuid7.New(),
            AiGovernanceId: AiGovernanceId.Value,
            PerformedBy: actorUserId.Value,
            OccurredAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Archives the AI governance record (Closed -&gt; Archived). Valid only from <see cref="AiGovernanceStatus.Closed"/>.</summary>
    public void Archive(
        UserId actorUserId, UtcTimestamp atUtc, PerformedByActor actor,
        CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AiGovernanceLifecycleRules.CanArchive, AiGovernanceStatus.Closed);

        Status = AiGovernanceStatus.Archived;

        _domainEvents.Add(new AiGovernanceArchivedEvent(
            EventId: Uuid7.New(),
            AiGovernanceId: AiGovernanceId.Value,
            PerformedBy: actorUserId.Value,
            OccurredAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<AiGovernanceStatus, bool> guard, AiGovernanceStatus expected)
    {
        if (guard(Status)) return;

        throw new AiGovernanceInvalidStatusTransitionException(
            AiGovernanceId.Value, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
