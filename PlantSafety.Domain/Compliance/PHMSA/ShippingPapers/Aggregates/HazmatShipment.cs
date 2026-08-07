// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           HazmatShipment.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the PHMSA ShippingPapers bounded context. Tracks a hazmat
//                  shipment: the classified materials and authorized packagings it contains, the
//                  generated shipping paper and emergency response information, transport mode,
//                  and the computed placarding/segregation results carried over from the
//                  Placarding and Segregation domain services.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Segregation.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Segregation.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Enums;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Events;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Exceptions;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Rules;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Aggregates;

/// <summary>Aggregate root for the PHMSA ShippingPapers bounded context.</summary>
public sealed class HazmatShipment
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this HazmatShipment aggregate.</summary>
    public HazmatShipmentId Id { get; private set; }

    /// <summary>
    /// Tenant that owns this shipment record. Added additively (2026-08-07, Application/
    /// Infrastructure/API build-out pass) so the Infrastructure layer can satisfy the
    /// tenant-scoped, NOT NULL TenantId column on [Compliance].[PHMSA_HazmatShipment] --
    /// mirrors every other tenant-scoped aggregate root in this codebase (e.g.
    /// PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.TenantId).
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; private set; }

    private List<HazmatMaterialId> _materialIds = [];

    /// <summary>Classified materials contained in this shipment (Classification module).</summary>
    public IReadOnlyList<HazmatMaterialId> MaterialIds => new ReadOnlyCollection<HazmatMaterialId>(_materialIds);

    private List<PackagingSpecificationId> _packagingIds = [];

    /// <summary>Authorized packaging specifications used for this shipment (Packaging module).</summary>
    public IReadOnlyList<PackagingSpecificationId> PackagingIds => new ReadOnlyCollection<PackagingSpecificationId>(_packagingIds);

    /// <summary>Generated shipping paper for this shipment, null until <see cref="AttachShippingPaper"/>.</summary>
    public ShippingPaper? ShippingPaper { get; private set; }

    /// <summary>Emergency response information for this shipment, null until <see cref="MarkReady"/>.</summary>
    public EmergencyResponseInfo? EmergencyResponseInfo { get; private set; }

    /// <summary>Mode of transport for this shipment.</summary>
    public TransportMode Mode { get; private set; }

    private List<string> _placardingRequirements = [];

    /// <summary>Computed placard names for this shipment (result of PlacardingEngine, stored here by the Application layer).</summary>
    public IReadOnlyList<string> PlacardingRequirements => new ReadOnlyCollection<string>(_placardingRequirements);

    private List<SegregationViolation> _segregationViolations = [];

    /// <summary>Recorded segregation violations for this shipment (result of SegregationEngine).</summary>
    public IReadOnlyList<SegregationViolation> SegregationViolations => new ReadOnlyCollection<SegregationViolation>(_segregationViolations);

    // ---- Audit / lifecycle -----------------------------------------------------

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public HazmatShipmentStatus Status { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IHazmatShipmentDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IHazmatShipmentDomainEvent> DomainEvents
        => new ReadOnlyCollection<IHazmatShipmentDomainEvent>(_domainEvents);

    // For rehydration only.
    private HazmatShipment() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>
    /// Creates a new HazmatShipment aggregate and raises <see cref="HazmatShipmentCreatedEvent"/>.
    /// Requires at least one material.
    /// </summary>
    public static HazmatShipment Create(
        HazmatShipmentId id,
        Guid tenantId,
        Guid? siteId,
        IReadOnlyList<HazmatMaterialId> materialIds,
        IReadOnlyList<PackagingSpecificationId> packagingIds,
        TransportMode mode,
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        if (materialIds is null || materialIds.Count == 0)
            throw new ShipmentNotReadyForDispatchException(id, "at least one material is required to create a shipment.", correlationId, causationId);

        var agg = new HazmatShipment
        {
            Id = id,
            TenantId = tenantId,
            SiteId = siteId,
            _materialIds = [.. materialIds],
            _packagingIds = [.. packagingIds ?? []],
            Mode = mode,
            CreatedBy = createdBy,
            CreatedAtUtc = createdAtUtc,
            Status = HazmatShipmentStatus.Draft,
        };

        agg.AddDomainEvent(new HazmatShipmentCreatedEvent(
            EventId: HazmatShipmentEventId.New(),
            HazmatShipmentId: agg.Id,
            MaterialIds: agg._materialIds,
            PackagingIds: agg._packagingIds,
            Mode: mode,
            CreatedAtUtc: createdAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return agg;
    }

    // ---- RecordSegregationEvaluation -------------------------------------------

    /// <summary>
    /// Records the result of a segregation evaluation (computed by SegregationEngine at the
    /// Application layer) against this shipment. Does not itself change lifecycle status; raises
    /// <see cref="ShipmentSegregationViolationsDetectedEvent"/> when any violations are present.
    /// </summary>
    public void RecordSegregationEvaluation(
        IReadOnlyList<SegregationViolation> violations,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        _segregationViolations = [.. violations ?? []];

        if (_segregationViolations.Count > 0)
        {
            AddDomainEvent(new ShipmentSegregationViolationsDetectedEvent(
                EventId: HazmatShipmentEventId.New(),
                HazmatShipmentId: Id,
                Violations: _segregationViolations,
                DetectedAtUtc: UtcTimestamp.Now(),
                Actor: actor,
                CorrelationId: correlationId,
                CausationId: causationId));
        }
    }

    // ---- RecordPlacardingRequirements ------------------------------------------

    /// <summary>
    /// Records the computed placard set (from PlacardingEngine at the Application layer) against
    /// this shipment.
    /// </summary>
    public void RecordPlacardingRequirements(IReadOnlyList<string> placards)
    {
        _placardingRequirements = [.. placards ?? []];
    }

    // ---- Lifecycle: AttachShippingPaper -----------------------------------------

    /// <summary>
    /// Attaches a generated shipping paper to this shipment (Draft/Validated -&gt; Validated).
    /// </summary>
    public void AttachShippingPaper(
        ShippingPaper paper,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        EnsureCanTransition(correlationId, causationId, HazmatShipmentLifecycleRules.CanAttachShippingPaper, HazmatShipmentStatus.Validated);

        ShippingPaper = paper ?? throw new ArgumentNullException(nameof(paper));
        Status = HazmatShipmentStatus.Validated;

        AddDomainEvent(new ShippingPaperGeneratedEvent(
            EventId: HazmatShipmentEventId.New(),
            HazmatShipmentId: Id,
            BasicDescriptionSequence: paper!.BasicDescriptionSequence,
            GeneratedAtUtc: paper.GeneratedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: MarkReady ---------------------------------------------------

    /// <summary>
    /// Marks the shipment ready for dispatch (Validated -&gt; Ready). Requires a shipping paper
    /// already attached and zero Prohibited-severity segregation violations.
    /// </summary>
    public void MarkReady(
        EmergencyResponseInfo emergencyResponseInfo,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        EnsureCanTransition(correlationId, causationId, HazmatShipmentLifecycleRules.CanMarkReady, HazmatShipmentStatus.Ready);

        if (ShippingPaper is null)
            throw new ShipmentNotReadyForDispatchException(Id, "no shipping paper has been attached.", correlationId, causationId);

        if (_segregationViolations.Any(v => v.Severity == SegregationSeverity.Prohibited))
            throw new ShipmentNotReadyForDispatchException(Id, "unresolved Prohibited-severity segregation violations remain.", correlationId, causationId);

        EmergencyResponseInfo = emergencyResponseInfo ?? throw new ArgumentNullException(nameof(emergencyResponseInfo));
        Status = HazmatShipmentStatus.Ready;

        AddDomainEvent(new HazmatShipmentReadyEvent(
            EventId: HazmatShipmentEventId.New(),
            HazmatShipmentId: Id,
            ReadyAtUtc: UtcTimestamp.Now(),
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Dispatch -----------------------------------------------------

    /// <summary>Dispatches the shipment (Ready -&gt; Dispatched).</summary>
    public void Dispatch(
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        EnsureCanTransition(correlationId, causationId, HazmatShipmentLifecycleRules.CanDispatch, HazmatShipmentStatus.Dispatched);

        Status = HazmatShipmentStatus.Dispatched;

        AddDomainEvent(new HazmatShipmentDispatchedEvent(
            EventId: HazmatShipmentEventId.New(),
            HazmatShipmentId: Id,
            DispatchedAtUtc: UtcTimestamp.Now(),
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Cancel -------------------------------------------------------

    /// <summary>Cancels the shipment from any non-terminal status.</summary>
    public void Cancel(
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        EnsureCanTransition(correlationId, causationId, HazmatShipmentLifecycleRules.CanCancel, HazmatShipmentStatus.Cancelled);

        Status = HazmatShipmentStatus.Cancelled;

        AddDomainEvent(new HazmatShipmentCancelledEvent(
            EventId: HazmatShipmentEventId.New(),
            HazmatShipmentId: Id,
            CancelledAtUtc: UtcTimestamp.Now(),
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IHazmatShipmentDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureCanTransition(CorrelationId correlationId, CausationId causationId, Func<HazmatShipmentStatus, bool> guard, HazmatShipmentStatus target)
    {
        if (guard(Status)) return;

        throw new HazmatShipmentInvalidStatusTransitionException(
            Id, Status.ToString(), target.ToString(), correlationId, causationId);
    }
}
