// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Domain
//  File:           WorkerWellBeing.cs
//  Created:        2026-06-17
//  Updated:        2026-08-02 -- Phase 4 full build-out: added SiteId, business fields
//                  (CheckInType/WellBeingScore/FollowUpRequired), replaced the ad-hoc string
//                  Status/WellBeingRiskLevel workflow fields' status with the generic
//                  WorkerWellBeingStatus lifecycle shape, and added the Activate/Complete/Close/
//                  Archive lifecycle methods. Mirrors Compliance.Auditing's full build-out
//                  depth/shape. Additive-only evolution (WellBeingRiskLevel retained unchanged).
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the WorkerWellBeing module. Tracks a single worker
//                  wellbeing/mental-health check-in record (risk classification, score, and
//                  workflow status) per tenant.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.WorkerWellBeing.Aggregates;

/// <summary>Aggregate root for the WorkerWellBeing bounded context.</summary>
public sealed class WorkerWellBeing
{
    /// <summary>Unique identity of this WorkerWellBeing aggregate.</summary>
    public WorkerWellBeingId WorkerWellBeingId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Worker this wellbeing record relates to.</summary>
    public UserId WorkerId { get; private set; }

    /// <summary>Worker this check-in record relates to. Alias of <see cref="WorkerId"/> using the Application-facing name.</summary>
    public UserId WorkerUserId => WorkerId;

    /// <summary>Wellbeing risk classification: Low, Medium, High, Critical.</summary>
    public string WellBeingRiskLevel { get; private set; } = "Low";

    /// <summary>Check-in classification (e.g., MentalHealth, PhysicalHealth, WorkLifeBalance).</summary>
    public string CheckInType { get; private set; } = string.Empty;

    /// <summary>Self-reported or assessed wellbeing score, 1-10.</summary>
    public int WellBeingScore { get; private set; }

    /// <summary>Whether this check-in requires follow-up.</summary>
    public bool FollowUpRequired { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public WorkerWellBeingStatus Status { get; private set; } = WorkerWellBeingStatus.Created;

    /// <summary>UTC timestamp this assessment was recorded.</summary>
    public UtcTimestamp RecordedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // Domain events
    private readonly List<IWorkerWellBeingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IWorkerWellBeingDomainEvent> DomainEvents
        => new ReadOnlyCollection<IWorkerWellBeingDomainEvent>(_domainEvents);

    // For rehydration only.
    private WorkerWellBeing() { }

    /// <summary>Creates a new WorkerWellBeing aggregate and raises <see cref="WorkerWellBeingCreatedEvent"/>.</summary>
    /// <param name="tenantId">Owning tenant. Required.</param>
    /// <param name="siteId">Optional owning site.</param>
    /// <param name="workerId">Worker this assessment relates to.</param>
    /// <param name="wellBeingRiskLevel">Risk classification (Low/Medium/High/Critical). Must not be null or whitespace.</param>
    /// <param name="checkInType">Check-in classification (e.g., MentalHealth/PhysicalHealth/WorkLifeBalance). Must not be null or whitespace.</param>
    /// <param name="wellBeingScore">Self-reported or assessed wellbeing score, 1-10.</param>
    /// <param name="followUpRequired">Whether this check-in requires follow-up.</param>
    /// <param name="createdBy">CRUD authorship identity.</param>
    /// <param name="createdAtUtc">Creation timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="tenantId"/> is empty, <paramref name="wellBeingRiskLevel"/> or
    /// <paramref name="checkInType"/> is null or whitespace, or <paramref name="wellBeingScore"/>
    /// is outside the 1-10 range.
    /// </exception>
    public static WorkerWellBeing Create(
        Guid tenantId, Guid? siteId, UserId workerId, string wellBeingRiskLevel,
        string checkInType, int wellBeingScore, bool followUpRequired,
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId must not be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(wellBeingRiskLevel))
            throw new ArgumentException("WellBeingRiskLevel must not be null or whitespace.", nameof(wellBeingRiskLevel));
        if (string.IsNullOrWhiteSpace(checkInType))
            throw new ArgumentException("CheckInType must not be null or whitespace.", nameof(checkInType));
        if (wellBeingScore is < 1 or > 10)
            throw new ArgumentOutOfRangeException(nameof(wellBeingScore), "WellBeingScore must be between 1 and 10.");

        var agg = new WorkerWellBeing
        {
            WorkerWellBeingId  = WorkerWellBeingId.New(),
            TenantId           = tenantId,
            SiteId             = siteId,
            WorkerId           = workerId,
            WellBeingRiskLevel = wellBeingRiskLevel.Trim(),
            CheckInType        = checkInType.Trim(),
            WellBeingScore     = wellBeingScore,
            FollowUpRequired   = followUpRequired,
            Status             = WorkerWellBeingStatus.Created,
            RecordedAtUtc      = createdAtUtc,
            CreatedAtUtc       = createdAtUtc,
            CreatedBy          = createdBy,
        };

        agg._domainEvents.Add(new WorkerWellBeingCreatedEvent(
            EventId:            Uuid7.New(),
            WorkerWellBeingId:  agg.WorkerWellBeingId.Value,
            TenantId:           tenantId,
            WorkerId:           workerId.Value,
            WellBeingRiskLevel: agg.WellBeingRiskLevel,
            CreatedAtUtc:       createdAtUtc,
            Actor:              actor,
            CorrelationId:      correlationId,
            CausationId:        causationId));

        return agg;
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    // ---- Lifecycle: Activate / Complete / Close / Archive -------------------

    /// <summary>
    /// Activates a newly created check-in record (Created -&gt; Active). Valid only from
    /// <see cref="WorkerWellBeingStatus.Created"/>.
    /// </summary>
    public void Activate(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, WorkerWellBeingLifecycleRules.CanActivate, WorkerWellBeingStatus.Created);

        Status = WorkerWellBeingStatus.Active;

        AddDomainEvent(new WorkerWellBeingActivatedEvent(
            EventId: Uuid7.New(),
            WorkerWellBeingId: WorkerWellBeingId.Value,
            ActivatedByUserId: actorUserId.Value,
            ActivatedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Completes an active check-in record (Active -&gt; Completed). Valid only from
    /// <see cref="WorkerWellBeingStatus.Active"/>.
    /// </summary>
    public void Complete(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, WorkerWellBeingLifecycleRules.CanComplete, WorkerWellBeingStatus.Active);

        Status = WorkerWellBeingStatus.Completed;

        AddDomainEvent(new WorkerWellBeingCompletedEvent(
            EventId: Uuid7.New(),
            WorkerWellBeingId: WorkerWellBeingId.Value,
            CompletedByUserId: actorUserId.Value,
            CompletedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Formally closes a completed check-in record (Completed -&gt; Closed). Valid only from
    /// <see cref="WorkerWellBeingStatus.Completed"/>.
    /// </summary>
    public void Close(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, WorkerWellBeingLifecycleRules.CanClose, WorkerWellBeingStatus.Completed);

        Status = WorkerWellBeingStatus.Closed;

        AddDomainEvent(new WorkerWellBeingClosedEvent(
            EventId: Uuid7.New(),
            WorkerWellBeingId: WorkerWellBeingId.Value,
            ClosedByUserId: actorUserId.Value,
            ClosedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Archives a closed check-in record for compliance retention (Closed -&gt; Archived). Valid
    /// only from <see cref="WorkerWellBeingStatus.Closed"/>.
    /// </summary>
    public void Archive(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, WorkerWellBeingLifecycleRules.CanArchive, WorkerWellBeingStatus.Closed);

        Status = WorkerWellBeingStatus.Archived;

        AddDomainEvent(new WorkerWellBeingArchivedEvent(
            EventId: Uuid7.New(),
            WorkerWellBeingId: WorkerWellBeingId.Value,
            ArchivedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers --------------------------------------------------------------

    private void AddDomainEvent(IWorkerWellBeingDomainEvent evt) => _domainEvents.Add(evt);

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<WorkerWellBeingStatus, bool> guard, WorkerWellBeingStatus expected)
    {
        if (guard(Status)) return;

        throw new WorkerWellBeingInvalidStatusTransitionException(
            WorkerWellBeingId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
