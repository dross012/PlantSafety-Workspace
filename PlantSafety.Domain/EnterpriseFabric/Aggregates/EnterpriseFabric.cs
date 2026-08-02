// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Domain
//  File:           EnterpriseFabric.cs
//  Created:        2026-06-17
//  Updated:        2026-08-02 -- Phase 4 full build-out: extended from an identity-only scaffold
//                  with the full enterprise-integration/system-connectivity record shape (tenant
//                  scope, fabric area, system reference, integration type, owner) plus the
//                  Created -> Active -> Completed -> Closed -> Archived lifecycle. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing's identical
//                  build-out shape.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the EnterpriseFabric bounded context. Tracks a single
//                  enterprise-integration/system-connectivity record: its integration area,
//                  external system reference, integration mechanism, owner, and lifecycle status.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnterpriseFabric.Aggregates;

/// <summary>Aggregate root for the EnterpriseFabric bounded context.</summary>
public sealed class EnterpriseFabric
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Unique identity of this EnterpriseFabric aggregate.</summary>
    public EnterpriseFabricId EnterpriseFabricId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public EnterpriseFabricStatus Status { get; private set; }

    // ---- Enterprise-integration business properties --------------------------

    /// <summary>Integration area classification (e.g. IntegrationHub/MasterDataSync/IdentityFederation).</summary>
    public string FabricArea { get; private set; } = string.Empty;

    /// <summary>Name of the external system/endpoint this record connects to.</summary>
    public string SystemReference { get; private set; } = string.Empty;

    /// <summary>Integration mechanism (e.g. Api/EventBus/FileTransfer/Database).</summary>
    public string IntegrationType { get; private set; } = string.Empty;

    /// <summary>Identity of the user who owns this integration record.</summary>
    public UserId OwnerUserId { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IEnterpriseFabricDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IEnterpriseFabricDomainEvent> DomainEvents
        => new ReadOnlyCollection<IEnterpriseFabricDomainEvent>(_domainEvents);

    // For rehydration only.
    private EnterpriseFabric() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new EnterpriseFabric aggregate and raises <see cref="EnterpriseFabricCreatedEvent"/>.</summary>
    /// <param name="createdBy">CRUD authorship identity.</param>
    /// <param name="createdAtUtc">Creation timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    /// <param name="tenantId">Owning tenant. Required.</param>
    /// <param name="siteId">Optional owning site.</param>
    /// <param name="fabricArea">Integration area classification.</param>
    /// <param name="systemReference">External system/endpoint name.</param>
    /// <param name="integrationType">Integration mechanism (Api/EventBus/FileTransfer/Database).</param>
    /// <param name="ownerUserId">Owning user identity.</param>
    public static EnterpriseFabric Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string fabricArea = "",
        string systemReference = "",
        string integrationType = "",
        UserId ownerUserId = default)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required.", nameof(tenantId));

        var agg = new EnterpriseFabric
        {
            EnterpriseFabricId = EnterpriseFabricId.New(),
            TenantId           = tenantId,
            SiteId             = siteId,
            Status             = EnterpriseFabricStatus.Created,
            FabricArea         = (fabricArea ?? string.Empty).Trim(),
            SystemReference    = (systemReference ?? string.Empty).Trim(),
            IntegrationType    = (integrationType ?? string.Empty).Trim(),
            OwnerUserId        = ownerUserId,
            CreatedAtUtc       = createdAtUtc,
            CreatedBy          = createdBy,
        };

        agg._domainEvents.Add(new EnterpriseFabricCreatedEvent(
            EventId:       Uuid7.New(),
            EnterpriseFabricId: agg.EnterpriseFabricId.Value,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: Activate / Complete / Close / Archive -------------------
    // Phase 4 full build-out (2026-08-02): this aggregate previously had only the Create
    // factory. Mirrors Compliance.Auditing's Start/RecordFindings/Close/Archive build-out shape.

    /// <summary>
    /// Activates the EnterpriseFabric record (Created -&gt; Active). Valid only from
    /// <see cref="EnterpriseFabricStatus.Created"/>.
    /// </summary>
    public void Activate(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EnterpriseFabricLifecycleRules.CanActivate, EnterpriseFabricStatus.Created);

        Status = EnterpriseFabricStatus.Active;

        AddDomainEvent(new EnterpriseFabricActivatedEvent(
            EventId: Uuid7.New(),
            EnterpriseFabricId: EnterpriseFabricId.Value,
            ActivatedBy: actorUserId.Value,
            ActivatedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Completes the EnterpriseFabric record (Active -&gt; Completed). Valid only from
    /// <see cref="EnterpriseFabricStatus.Active"/>.
    /// </summary>
    public void Complete(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EnterpriseFabricLifecycleRules.CanComplete, EnterpriseFabricStatus.Active);

        Status = EnterpriseFabricStatus.Completed;

        AddDomainEvent(new EnterpriseFabricCompletedEvent(
            EventId: Uuid7.New(),
            EnterpriseFabricId: EnterpriseFabricId.Value,
            CompletedBy: actorUserId.Value,
            CompletedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Closes the EnterpriseFabric record (Completed -&gt; Closed). Valid only from
    /// <see cref="EnterpriseFabricStatus.Completed"/>.
    /// </summary>
    public void Close(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EnterpriseFabricLifecycleRules.CanClose, EnterpriseFabricStatus.Completed);

        Status = EnterpriseFabricStatus.Closed;

        AddDomainEvent(new EnterpriseFabricClosedEvent(
            EventId: Uuid7.New(),
            EnterpriseFabricId: EnterpriseFabricId.Value,
            ClosedBy: actorUserId.Value,
            ClosedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Archives the EnterpriseFabric record (Closed -&gt; Archived). Valid only from
    /// <see cref="EnterpriseFabricStatus.Closed"/>.
    /// </summary>
    public void Archive(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EnterpriseFabricLifecycleRules.CanArchive, EnterpriseFabricStatus.Closed);

        Status = EnterpriseFabricStatus.Archived;

        AddDomainEvent(new EnterpriseFabricArchivedEvent(
            EventId: Uuid7.New(),
            EnterpriseFabricId: EnterpriseFabricId.Value,
            ArchivedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IEnterpriseFabricDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<EnterpriseFabricStatus, bool> guard, EnterpriseFabricStatus expected)
    {
        if (guard(Status)) return;

        throw new EnterpriseFabricInvalidStatusTransitionException(
            EnterpriseFabricId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
