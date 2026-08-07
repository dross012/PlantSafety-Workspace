// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Domain
//  File:           HazmatContainer.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the PHMSA Placarding (Marking/Labeling/Placarding) bounded
//                  context. Tracks a single hazmat container/load: its referenced classified
//                  material and authorized packaging, the text markings and hazard labels applied
//                  to it, and the transport placards required once bulk/quantity thresholds are
//                  evaluated (see Services.PlacardingEngine).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Events;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Exceptions;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Rules;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Placarding.Aggregates;

/// <summary>Aggregate root for the PHMSA Placarding bounded context.</summary>
public sealed class HazmatContainer
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this HazmatContainer aggregate.</summary>
    public HazmatContainerId Id { get; private set; }

    /// <summary>
    /// Tenant that owns this container record. Added additively (2026-08-07, Application/
    /// Infrastructure/API build-out pass) so the Infrastructure layer can satisfy the
    /// tenant-scoped, NOT NULL TenantId column on [Compliance].[PHMSA_HazmatContainer] --
    /// mirrors every other tenant-scoped aggregate root in this codebase (e.g.
    /// PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.TenantId).
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Physical container/packaging form factor.</summary>
    public ContainerType ContainerType { get; private set; }

    /// <summary>Reference to the classified material inside this container (Classification module).</summary>
    public HazmatMaterialId MaterialId { get; private set; }

    /// <summary>Reference to the authorized packaging specification (Packaging module).</summary>
    public PackagingSpecificationId PackagingSpecId { get; private set; }

    // ---- Marking / Labeling / Placarding --------------------------------------

    private List<string> _markings = [];

    /// <summary>Text markings applied (e.g. "UN1090", proper shipping name, "RQ").</summary>
    public IReadOnlyList<string> Markings => new ReadOnlyCollection<string>(_markings);

    private List<string> _labels = [];

    /// <summary>Hazard label names (e.g. "Flammable Liquid", "Corrosive", "Marine Pollutant").</summary>
    public IReadOnlyList<string> Labels => new ReadOnlyCollection<string>(_labels);

    private List<string> _placards = [];

    /// <summary>Placard names required if this container/load meets bulk thresholds.</summary>
    public IReadOnlyList<string> Placards => new ReadOnlyCollection<string>(_placards);

    /// <summary>Whether orientation arrows are required on this container's packaging.</summary>
    public bool OrientationArrowsRequired { get; private set; }

    /// <summary>Explosives compatibility group letter (A-N, S). Only meaningful for Class 1 materials.</summary>
    public string? CompatibilityGroup { get; private set; }

    // ---- Audit / lifecycle -----------------------------------------------------

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public HazmatContainerStatus Status { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IHazmatContainerDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IHazmatContainerDomainEvent> DomainEvents
        => new ReadOnlyCollection<IHazmatContainerDomainEvent>(_domainEvents);

    // For rehydration only.
    private HazmatContainer() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new HazmatContainer aggregate and raises <see cref="HazmatContainerCreatedEvent"/>.</summary>
    public static HazmatContainer Create(
        HazmatContainerId id,
        Guid tenantId,
        Guid? siteId,
        ContainerType containerType,
        HazmatMaterialId materialId,
        PackagingSpecificationId packagingSpecId,
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new HazmatContainer
        {
            Id = id,
            TenantId = tenantId,
            SiteId = siteId,
            ContainerType = containerType,
            MaterialId = materialId,
            PackagingSpecId = packagingSpecId,
            CreatedBy = createdBy,
            CreatedAtUtc = createdAtUtc,
            Status = HazmatContainerStatus.Draft,
        };

        agg.AddDomainEvent(new HazmatContainerCreatedEvent(
            EventId: HazmatContainerEventId.New(),
            HazmatContainerId: agg.Id,
            ContainerType: containerType,
            MaterialId: materialId,
            PackagingSpecId: packagingSpecId,
            CreatedAtUtc: createdAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return agg;
    }

    // ---- Lifecycle: ApplyMarkingsAndLabels -----------------------------------

    /// <summary>
    /// Applies text markings and hazard labels to this container (Draft -&gt; MarkedAndLabeled).
    /// Valid only from <see cref="HazmatContainerStatus.Draft"/>.
    /// </summary>
    public void ApplyMarkingsAndLabels(
        IReadOnlyList<string> markings,
        IReadOnlyList<string> labels,
        bool orientationArrowsRequired,
        string? compatibilityGroup,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        EnsureCanTransition(correlationId, causationId, HazmatContainerLifecycleRules.CanApplyMarkingsAndLabels, HazmatContainerStatus.MarkedAndLabeled);

        _markings = [.. markings ?? []];
        _labels = [.. labels ?? []];
        OrientationArrowsRequired = orientationArrowsRequired;
        CompatibilityGroup = string.IsNullOrWhiteSpace(compatibilityGroup) ? null : compatibilityGroup.Trim();
        Status = HazmatContainerStatus.MarkedAndLabeled;

        var markedAtUtc = UtcTimestamp.Now();
        AddDomainEvent(new HazmatContainerMarkedEvent(
            EventId: HazmatContainerEventId.New(),
            HazmatContainerId: Id,
            Markings: _markings,
            Labels: _labels,
            OrientationArrowsRequired: OrientationArrowsRequired,
            CompatibilityGroup: CompatibilityGroup,
            MarkedAtUtc: markedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: ApplyPlacards ---------------------------------------------

    /// <summary>
    /// Records the computed placard set for this container (MarkedAndLabeled -&gt; PlacardingCalculated).
    /// Valid only from <see cref="HazmatContainerStatus.MarkedAndLabeled"/>.
    /// </summary>
    public void ApplyPlacards(
        IReadOnlyList<string> placards,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        EnsureCanTransition(correlationId, causationId, HazmatContainerLifecycleRules.CanApplyPlacards, HazmatContainerStatus.PlacardingCalculated);

        _placards = [.. placards ?? []];
        Status = HazmatContainerStatus.PlacardingCalculated;

        var placardedAtUtc = UtcTimestamp.Now();
        AddDomainEvent(new HazmatContainerPlacardedEvent(
            EventId: HazmatContainerEventId.New(),
            HazmatContainerId: Id,
            Placards: _placards,
            PlacardedAtUtc: placardedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Retire ------------------------------------------------------

    /// <summary>Retires this container record from active use.</summary>
    public void Retire(
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        EnsureCanTransition(correlationId, causationId, HazmatContainerLifecycleRules.CanRetire, HazmatContainerStatus.Retired);

        Status = HazmatContainerStatus.Retired;

        var retiredAtUtc = UtcTimestamp.Now();
        AddDomainEvent(new HazmatContainerRetiredEvent(
            EventId: HazmatContainerEventId.New(),
            HazmatContainerId: Id,
            RetiredAtUtc: retiredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IHazmatContainerDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureCanTransition(CorrelationId correlationId, CausationId causationId, Func<HazmatContainerStatus, bool> guard, HazmatContainerStatus target)
    {
        if (guard(Status)) return;

        throw new HazmatContainerInvalidStatusTransitionException(
            Id, Status.ToString(), target.ToString(), correlationId, causationId);
    }
}
