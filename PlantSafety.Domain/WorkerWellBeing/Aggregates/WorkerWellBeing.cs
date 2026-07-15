// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Domain
//  File:           WorkerWellBeing.cs
//  Created:        2026-06-17
//  Updated:        2026-07-15 — extended from identity-only scaffold with the full
//                  WorkerWellBeing record shape (WorkerId, risk classification,
//                  workflow status) to back the Application/Api CQRS slice.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the WorkerWellBeing module. Tracks a single worker
//                  wellbeing assessment record (fatigue/stress-derived risk classification
//                  and its workflow status) per tenant.
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

    /// <summary>Worker this wellbeing record relates to.</summary>
    public UserId WorkerId { get; private set; }

    /// <summary>Wellbeing risk classification: Low, Medium, High, Critical.</summary>
    public string WellBeingRiskLevel { get; private set; } = "Low";

    /// <summary>Workflow status: Open, UnderReview, Actioned, Closed.</summary>
    public string Status { get; private set; } = "Open";

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

    private WorkerWellBeing() { }

    /// <summary>Creates a new WorkerWellBeing aggregate and raises <see cref="WorkerWellBeingCreatedEvent"/>.</summary>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="workerId">Worker this assessment relates to.</param>
    /// <param name="wellBeingRiskLevel">Risk classification (Low/Medium/High/Critical). Must not be null or whitespace.</param>
    /// <param name="createdBy">CRUD authorship identity.</param>
    /// <param name="createdAtUtc">Creation timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="wellBeingRiskLevel"/> is null or whitespace.</exception>
    public static WorkerWellBeing Create(
        Guid tenantId, UserId workerId, string wellBeingRiskLevel,
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor)
    {
        if (string.IsNullOrWhiteSpace(wellBeingRiskLevel))
            throw new ArgumentException("WellBeingRiskLevel must not be null or whitespace.", nameof(wellBeingRiskLevel));

        var agg = new WorkerWellBeing
        {
            WorkerWellBeingId  = WorkerWellBeingId.New(),
            TenantId           = tenantId,
            WorkerId           = workerId,
            WellBeingRiskLevel = wellBeingRiskLevel.Trim(),
            Status             = "Open",
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
}
