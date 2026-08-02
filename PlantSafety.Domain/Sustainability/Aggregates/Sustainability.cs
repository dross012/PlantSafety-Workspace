// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Domain
//  File:           Sustainability.cs
//  Created:        2026-06-17
//  Updated:        2026-08-02 -- Phase 4 full build-out: extended from identity-only scaffold
//                  with TenantId/SiteId, business fields (InitiativeName/Category/TargetValue/
//                  OwnerUserId), the generic SustainabilityStatus lifecycle shape, and the
//                  Activate/Complete/Close/Archive lifecycle methods. Mirrors
//                  Compliance.Auditing's full build-out depth/shape. Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Sustainability module. Tracks a single sustainability/
//                  ESG initiative: its category, target, owner, and lifecycle status.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.Aggregates;

/// <summary>Aggregate root for the Sustainability bounded context.</summary>
public sealed class Sustainability
{
    /// <summary>Unique identity of this Sustainability aggregate.</summary>
    public SustainabilityId SustainabilityId { get; private set; }

    /// <summary>Tenant that owns this initiative record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this initiative is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public SustainabilityStatus Status { get; private set; } = SustainabilityStatus.Created;

    /// <summary>Human-readable sustainability/ESG initiative name.</summary>
    public string InitiativeName { get; private set; } = string.Empty;

    /// <summary>Initiative category (e.g., Emissions, WasteReduction, EnergyEfficiency, WaterConservation).</summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>Optional target value for the initiative's tracked metric.</summary>
    public decimal? TargetValue { get; private set; }

    /// <summary>Identity of the accountable initiative owner.</summary>
    public UserId OwnerUserId { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // Domain events
    private readonly List<ISustainabilityDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ISustainabilityDomainEvent> DomainEvents
        => new ReadOnlyCollection<ISustainabilityDomainEvent>(_domainEvents);

    // For rehydration only.
    private Sustainability() { }

    /// <summary>Creates a new Sustainability aggregate and raises <see cref="SustainabilityCreatedEvent"/>.</summary>
    /// <param name="createdBy">CRUD authorship identity.</param>
    /// <param name="createdAtUtc">Creation timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    /// <param name="tenantId">Owning tenant. Required.</param>
    /// <param name="siteId">Optional owning site.</param>
    /// <param name="initiativeName">Human-readable initiative name. Must not be null or whitespace.</param>
    /// <param name="category">Initiative category. Must not be null or whitespace.</param>
    /// <param name="targetValue">Optional target value for the initiative's tracked metric.</param>
    /// <param name="ownerUserId">Accountable initiative owner.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="tenantId"/> is empty, or <paramref name="initiativeName"/> or <paramref name="category"/> is null or whitespace.</exception>
    public static Sustainability Create(
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor,
        Guid tenantId, Guid? siteId, string initiativeName, string category,
        decimal? targetValue, UserId ownerUserId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId must not be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(initiativeName))
            throw new ArgumentException("InitiativeName must not be null or whitespace.", nameof(initiativeName));
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category must not be null or whitespace.", nameof(category));

        var agg = new Sustainability
        {
            SustainabilityId = SustainabilityId.New(),
            TenantId         = tenantId,
            SiteId           = siteId,
            Status           = SustainabilityStatus.Created,
            InitiativeName   = initiativeName.Trim(),
            Category         = category.Trim(),
            TargetValue      = targetValue,
            OwnerUserId      = ownerUserId,
            CreatedAtUtc     = createdAtUtc,
            CreatedBy        = createdBy,
        };

        agg._domainEvents.Add(new SustainabilityCreatedEvent(
            EventId:         Uuid7.New(),
            SustainabilityId: agg.SustainabilityId.Value,
            TenantId:        tenantId,
            InitiativeName:  agg.InitiativeName,
            Category:        agg.Category,
            CreatedAtUtc:    createdAtUtc,
            Actor:           actor,
            CorrelationId:   correlationId,
            CausationId:     causationId));

        return agg;
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    // ---- Lifecycle: Activate / Complete / Close / Archive -------------------

    /// <summary>
    /// Activates a newly created initiative (Created -&gt; Active). Valid only from
    /// <see cref="SustainabilityStatus.Created"/>.
    /// </summary>
    public void Activate(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, SustainabilityLifecycleRules.CanActivate, SustainabilityStatus.Created);

        Status = SustainabilityStatus.Active;

        AddDomainEvent(new SustainabilityActivatedEvent(
            EventId: Uuid7.New(),
            SustainabilityId: SustainabilityId.Value,
            ActivatedByUserId: actorUserId.Value,
            ActivatedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Completes an active initiative (Active -&gt; Completed). Valid only from
    /// <see cref="SustainabilityStatus.Active"/>.
    /// </summary>
    public void Complete(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, SustainabilityLifecycleRules.CanComplete, SustainabilityStatus.Active);

        Status = SustainabilityStatus.Completed;

        AddDomainEvent(new SustainabilityCompletedEvent(
            EventId: Uuid7.New(),
            SustainabilityId: SustainabilityId.Value,
            CompletedByUserId: actorUserId.Value,
            CompletedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Formally closes a completed initiative (Completed -&gt; Closed). Valid only from
    /// <see cref="SustainabilityStatus.Completed"/>.
    /// </summary>
    public void Close(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, SustainabilityLifecycleRules.CanClose, SustainabilityStatus.Completed);

        Status = SustainabilityStatus.Closed;

        AddDomainEvent(new SustainabilityClosedEvent(
            EventId: Uuid7.New(),
            SustainabilityId: SustainabilityId.Value,
            ClosedByUserId: actorUserId.Value,
            ClosedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Archives a closed initiative for compliance retention (Closed -&gt; Archived). Valid only
    /// from <see cref="SustainabilityStatus.Closed"/>.
    /// </summary>
    public void Archive(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, SustainabilityLifecycleRules.CanArchive, SustainabilityStatus.Closed);

        Status = SustainabilityStatus.Archived;

        AddDomainEvent(new SustainabilityArchivedEvent(
            EventId: Uuid7.New(),
            SustainabilityId: SustainabilityId.Value,
            ArchivedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers --------------------------------------------------------------

    private void AddDomainEvent(ISustainabilityDomainEvent evt) => _domainEvents.Add(evt);

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<SustainabilityStatus, bool> guard, SustainabilityStatus expected)
    {
        if (guard(Status)) return;

        throw new SustainabilityInvalidStatusTransitionException(
            SustainabilityId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
