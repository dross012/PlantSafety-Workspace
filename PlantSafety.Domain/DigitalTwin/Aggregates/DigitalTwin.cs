// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Domain
//  File:           DigitalTwin.cs
//  Created:        2026-06-17
//  Updated:        2026-08-02 -- Phase 4 full build-out: extended from identity-only scaffold with
//                  TenantId/SiteId tenant-scoping, digital-twin model registry fields
//                  (TwinName/AssetReferenceId/ModelType/LastSyncedAtUtc), the full
//                  Created->Active->Completed->Closed->Archived lifecycle, and a same-status
//                  RefreshSync action (Active-scoped). This is a distinct concept from the
//                  pre-existing, unrelated "Twin" Features vertical
//                  (PlantSafety.Application.Features.DigitalTwin) -- not touched here. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing's build-out shape.
//                  Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the DigitalTwin module -- tracks a single digital-twin
//                  model registry record: its target asset, model type, and last synchronization
//                  timestamp.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.Aggregates;

/// <summary>Aggregate root for the DigitalTwin bounded context.</summary>
public sealed class DigitalTwin
{
    /// <summary>Unique identity of this DigitalTwin aggregate.</summary>
    public DigitalTwinId DigitalTwinId { get; private set; }

    /// <summary>Tenant that owns this digital-twin record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public DigitalTwinStatus Status { get; private set; }

    /// <summary>Name of the digital twin.</summary>
    public string TwinName { get; private set; } = string.Empty;

    /// <summary>Identity of the physical asset this twin represents, if known.</summary>
    public Guid? AssetReferenceId { get; private set; }

    /// <summary>Type of the underlying digital-twin model.</summary>
    public string ModelType { get; private set; } = string.Empty;

    /// <summary>UTC timestamp of the last successful synchronization.</summary>
    public UtcTimestamp? LastSyncedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // Domain events
    private readonly List<IDigitalTwinDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IDigitalTwinDomainEvent> DomainEvents
        => new ReadOnlyCollection<IDigitalTwinDomainEvent>(_domainEvents);

    private DigitalTwin() { }

    /// <summary>Creates a new DigitalTwin aggregate and raises <see cref="DigitalTwinCreatedEvent"/>.</summary>
    /// <param name="createdBy">CRUD authorship identity.</param>
    /// <param name="createdAtUtc">Creation timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="siteId">Optional owning site.</param>
    /// <param name="twinName">Name of the digital twin.</param>
    /// <param name="assetReferenceId">Identity of the physical asset represented, if known.</param>
    /// <param name="modelType">Type of the underlying digital-twin model.</param>
    public static DigitalTwin Create(
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string twinName = "",
        Guid? assetReferenceId = null,
        string modelType = "")
    {
        var agg = new DigitalTwin
        {
            DigitalTwinId       = DigitalTwinId.New(),
            TenantId            = tenantId,
            SiteId              = siteId,
            Status              = DigitalTwinStatus.Created,
            TwinName            = (twinName ?? string.Empty).Trim(),
            AssetReferenceId    = assetReferenceId,
            ModelType           = (modelType ?? string.Empty).Trim(),
            CreatedAtUtc        = createdAtUtc,
            CreatedBy           = createdBy,
        };

        agg._domainEvents.Add(new DigitalTwinCreatedEvent(
            EventId:       Uuid7.New(),
            DigitalTwinId:      agg.DigitalTwinId.Value,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: Activate / Complete / Close / Archive -------------------

    /// <summary>Activates the digital-twin record (Created -&gt; Active). Valid only from <see cref="DigitalTwinStatus.Created"/>.</summary>
    public void Activate(
        UserId actorUserId, UtcTimestamp atUtc, PerformedByActor actor,
        CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, DigitalTwinLifecycleRules.CanActivate, DigitalTwinStatus.Created);

        Status = DigitalTwinStatus.Active;

        _domainEvents.Add(new DigitalTwinActivatedEvent(
            EventId: Uuid7.New(),
            DigitalTwinId: DigitalTwinId.Value,
            PerformedBy: actorUserId.Value,
            OccurredAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Refreshes the last-synchronized timestamp for this digital twin. A same-status action --
    /// does not transition <see cref="Status"/>. Valid only while <see cref="DigitalTwinStatus.Active"/>.
    /// </summary>
    public void RefreshSync(
        UtcTimestamp syncedAtUtc, PerformedByActor actor,
        CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, s => s == DigitalTwinStatus.Active, DigitalTwinStatus.Active);

        LastSyncedAtUtc = syncedAtUtc;

        _domainEvents.Add(new DigitalTwinSyncRefreshedEvent(
            EventId: Uuid7.New(),
            DigitalTwinId: DigitalTwinId.Value,
            SyncedAtUtc: syncedAtUtc,
            OccurredAtUtc: syncedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Completes the digital-twin record (Active -&gt; Completed). Valid only from <see cref="DigitalTwinStatus.Active"/>.</summary>
    public void Complete(
        UserId actorUserId, UtcTimestamp atUtc, PerformedByActor actor,
        CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, DigitalTwinLifecycleRules.CanComplete, DigitalTwinStatus.Active);

        Status = DigitalTwinStatus.Completed;

        _domainEvents.Add(new DigitalTwinCompletedEvent(
            EventId: Uuid7.New(),
            DigitalTwinId: DigitalTwinId.Value,
            PerformedBy: actorUserId.Value,
            OccurredAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Closes the digital-twin record (Completed -&gt; Closed). Valid only from <see cref="DigitalTwinStatus.Completed"/>.</summary>
    public void Close(
        UserId actorUserId, UtcTimestamp atUtc, PerformedByActor actor,
        CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, DigitalTwinLifecycleRules.CanClose, DigitalTwinStatus.Completed);

        Status = DigitalTwinStatus.Closed;

        _domainEvents.Add(new DigitalTwinClosedEvent(
            EventId: Uuid7.New(),
            DigitalTwinId: DigitalTwinId.Value,
            PerformedBy: actorUserId.Value,
            OccurredAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Archives the digital-twin record (Closed -&gt; Archived). Valid only from <see cref="DigitalTwinStatus.Closed"/>.</summary>
    public void Archive(
        UserId actorUserId, UtcTimestamp atUtc, PerformedByActor actor,
        CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, DigitalTwinLifecycleRules.CanArchive, DigitalTwinStatus.Closed);

        Status = DigitalTwinStatus.Archived;

        _domainEvents.Add(new DigitalTwinArchivedEvent(
            EventId: Uuid7.New(),
            DigitalTwinId: DigitalTwinId.Value,
            PerformedBy: actorUserId.Value,
            OccurredAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<DigitalTwinStatus, bool> guard, DigitalTwinStatus expected)
    {
        if (guard(Status)) return;

        throw new DigitalTwinInvalidStatusTransitionException(
            DigitalTwinId.Value, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
