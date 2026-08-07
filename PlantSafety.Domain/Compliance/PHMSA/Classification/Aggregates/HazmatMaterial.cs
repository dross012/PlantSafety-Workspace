// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           HazmatMaterial.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the PHMSA Hazmat Classification bounded context. Tracks
//                  a single hazardous material's DOT/PHMSA classification: UN number, proper
//                  shipping name, hazard class, packing group, special provisions, ERG guide
//                  number, and Toxic Inhalation Hazard (TIH) status. Mirrors the
//                  Compliance.Permits aggregate's hand-rolled lifecycle pattern.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Classification.Events;
using PlantSafety.Domain.Compliance.PHMSA.Classification.Exceptions;
using PlantSafety.Domain.Compliance.PHMSA.Classification.Rules;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Classification.Aggregates;

/// <summary>Aggregate root for the PHMSA Hazmat Classification bounded context.</summary>
public sealed class HazmatMaterial
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this HazmatMaterial aggregate.</summary>
    public HazmatMaterialId Id { get; private set; }

    /// <summary>
    /// Tenant that owns this classification record. Added additively (2026-08-07, Application/
    /// Infrastructure/API build-out pass) so the Infrastructure layer can satisfy the
    /// tenant-scoped, NOT NULL TenantId column on [Compliance].[PHMSA_HazmatMaterial] -- mirrors
    /// every other tenant-scoped aggregate root in this codebase (e.g.
    /// PlantSafety.Domain.Compliance.Permits.Aggregates.Permits.TenantId and the identical
    /// precedent already applied to PlantSafety.Domain.Compliance.PHMSA.Placarding.Aggregates.
    /// HazmatContainer.TenantId in this same build-out pass).
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public HazmatMaterialStatus Status { get; private set; }

    // ---- DOT/PHMSA classification fields (49 CFR 172.101) ---------------------

    /// <summary>DOT/UN identification number, e.g. "UN1090".</summary>
    public UNNumber UNNumber { get; private set; }

    /// <summary>DOT proper shipping name, e.g. "Acetone".</summary>
    public ProperShippingName ProperShippingName { get; private set; }

    /// <summary>DOT hazard class/division, e.g. Class 3 or Class 6.1.</summary>
    public HazardClass HazardClass { get; private set; }

    /// <summary>DOT packing group (I/II/III), or NotApplicable when the class carries none.</summary>
    public PackingGroup PackingGroup { get; private set; }

    /// <summary>Free-text DOT special provision codes (e.g. "IB8", "T7", "TP2").</summary>
    public IReadOnlyList<string> SpecialProvisions { get; private set; } = [];

    /// <summary>Emergency Response Guidebook 3-digit guide number, if assigned.</summary>
    public string? ERGGuideNumber { get; private set; }

    /// <summary>Whether the material is classified as a marine pollutant.</summary>
    public bool IsMarinePollutant { get; private set; }

    /// <summary>Whether the material is a Toxic Inhalation Hazard (TIH/PIH).</summary>
    public bool IsTIH { get; private set; }

    /// <summary>TIH zone (A-D). Populated only when <see cref="IsTIH"/> is <see langword="true"/>.</summary>
    public TIHZone? TIHZone { get; private set; }

    // ---- Authorship / audit ----------------------------------------------------

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    // ---- Lifecycle attributes ---------------------------------------------------

    /// <summary>Identity of the HazmatMaterial that superseded this one, if any.</summary>
    public HazmatMaterialId? SupersededByMaterialId { get; private set; }

    /// <summary>UTC instant this classification was superseded.</summary>
    public UtcTimestamp? SupersededAtUtc { get; private set; }

    /// <summary>UTC instant this classification was retired.</summary>
    public UtcTimestamp? RetiredAtUtc { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IHazmatMaterialDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IHazmatMaterialDomainEvent> DomainEvents
        => new ReadOnlyCollection<IHazmatMaterialDomainEvent>(_domainEvents);

    // For rehydration only.
    private HazmatMaterial() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>
    /// Classifies a new HazmatMaterial and raises <see cref="HazmatMaterialClassifiedEvent"/>.
    /// Validates that <paramref name="isTIH"/> and <paramref name="tihZone"/> are consistent.
    /// </summary>
    public static HazmatMaterial Create(
        UNNumber unNumber,
        ProperShippingName shippingName,
        HazardClass hazardClass,
        PackingGroup packingGroup,
        IReadOnlyList<string> specialProvisions,
        string? ergGuideNumber,
        bool isMarinePollutant,
        bool isTIH,
        TIHZone? tihZone,
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId = null)
    {
        EnsureTIHZoneConsistent(isTIH, tihZone, correlationId, causationId);

        var agg = new HazmatMaterial
        {
            Id                 = HazmatMaterialId.New(),
            TenantId           = tenantId,
            SiteId             = siteId,
            Status             = HazmatMaterialStatus.Classified,
            UNNumber           = unNumber,
            ProperShippingName = shippingName,
            HazardClass        = hazardClass,
            PackingGroup       = packingGroup,
            SpecialProvisions  = specialProvisions ?? [],
            ERGGuideNumber     = string.IsNullOrWhiteSpace(ergGuideNumber) ? null : ergGuideNumber.Trim(),
            IsMarinePollutant  = isMarinePollutant,
            IsTIH              = isTIH,
            TIHZone            = tihZone,
            CreatedBy          = createdBy,
            CreatedAtUtc       = createdAtUtc,
        };

        agg.AddDomainEvent(new HazmatMaterialClassifiedEvent(
            HazmatMaterialId:   agg.Id,
            UNNumber:           unNumber,
            ProperShippingName: shippingName,
            HazardClass:        hazardClass,
            ClassifiedAtUtc:    createdAtUtc,
            Actor:              actor,
            CorrelationId:      correlationId,
            CausationId:        causationId));

        return agg;
    }

    // ---- Lifecycle: Supersede / Retire ---------------------------------------

    /// <summary>
    /// Marks this classification superseded by a newer classification record
    /// (Classified -&gt; Superseded). Valid only from <see cref="HazmatMaterialStatus.Classified"/>.
    /// </summary>
    public void Supersede(
        HazmatMaterialId supersededByMaterialId,
        UtcTimestamp supersededAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusGuard(correlationId, causationId, HazmatMaterialLifecycleRules.CanSupersede, "Superseded");

        SupersededByMaterialId = supersededByMaterialId;
        SupersededAtUtc = supersededAtUtc;
        Status = HazmatMaterialStatus.Superseded;

        AddDomainEvent(new HazmatMaterialSupersededEvent(
            HazmatMaterialId:        Id,
            SupersededByMaterialId:  supersededByMaterialId,
            SupersededAtUtc:         supersededAtUtc,
            Actor:                   actor,
            CorrelationId:           correlationId,
            CausationId:             causationId));
    }

    /// <summary>
    /// Retires this classification record (any non-Retired status -&gt; Retired).
    /// </summary>
    public void Retire(
        UtcTimestamp retiredAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusGuard(correlationId, causationId, HazmatMaterialLifecycleRules.CanRetire, "Retired");

        RetiredAtUtc = retiredAtUtc;
        Status = HazmatMaterialStatus.Retired;

        AddDomainEvent(new HazmatMaterialRetiredEvent(
            HazmatMaterialId: Id,
            RetiredAtUtc:     retiredAtUtc,
            Actor:            actor,
            CorrelationId:    correlationId,
            CausationId:      causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IHazmatMaterialDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private static void EnsureTIHZoneConsistent(bool isTIH, TIHZone? tihZone, CorrelationId correlationId, CausationId causationId)
    {
        if (isTIH && tihZone is null)
            throw new InvalidTIHZoneAssignmentException(isTIH, null, correlationId, causationId);
        if (!isTIH && tihZone is not null)
            throw new InvalidTIHZoneAssignmentException(isTIH, tihZone.ToString(), correlationId, causationId);
    }

    private void EnsureStatusGuard(CorrelationId correlationId, CausationId causationId, Func<HazmatMaterialStatus, bool> guard, string targetStatus)
    {
        if (guard(Status)) return;

        throw new HazmatMaterialInvalidStatusTransitionException(
            Id, Status.ToString(), targetStatus, correlationId, causationId);
    }
}
