// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.TsdfFacilities
//  Layer:          Domain
//  File:           TsdfFacility.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a permitted hazardous-waste Treatment, Storage, and
//                  Disposal Facility (TSDF). Integrates with Infrastructure.Integrations for
//                  TSDF EDI (electronic manifest receipt confirmation) hand-off.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Enums;
using PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Events;
using PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Aggregates;

/// <summary>Aggregate root for a permitted hazardous-waste Treatment, Storage, and Disposal Facility.</summary>
public sealed partial class TsdfFacility
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this TsdfFacility aggregate.</summary>
    public TsdfFacilityId TsdfFacilityId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public TsdfFacilityStatus Status { get; private set; }

    // ---- Facility detail -----------------------------------------------------

    /// <summary>Legal facility name.</summary>
    public string FacilityName { get; private set; } = string.Empty;

    /// <summary>EPA facility identification number.</summary>
    public string EpaFacilityId { get; private set; } = string.Empty;

    /// <summary>UTC timestamp the facility's RCRA permit expires.</summary>
    public UtcTimestamp? PermitExpiresAtUtc { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ITsdfFacilityDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ITsdfFacilityDomainEvent> DomainEvents
        => new ReadOnlyCollection<ITsdfFacilityDomainEvent>(_domainEvents);

    // For rehydration only.
    private TsdfFacility() { }

    // ---- Factory: Create (Register) -----------------------------------------------------------

    /// <summary>Registers a new TSDF facility and raises <see cref="TsdfFacilityRegisteredEvent"/>.</summary>
    public static TsdfFacility Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string facilityName = "",
        string epaFacilityId = "",
        UtcTimestamp? permitExpiresAtUtc = null)
    {
        var agg = new TsdfFacility
        {
            TsdfFacilityId        = TsdfFacilityId.New(),
            TenantId               = tenantId,
            SiteId                 = siteId,
            Status                 = TsdfFacilityStatus.Active,
            FacilityName           = (facilityName ?? string.Empty).Trim(),
            EpaFacilityId          = (epaFacilityId ?? string.Empty).Trim(),
            PermitExpiresAtUtc     = permitExpiresAtUtc,
            CreatedAtUtc           = createdAtUtc,
            CreatedBy              = createdBy,
        };

        agg.AddDomainEvent(new TsdfFacilityRegisteredEvent(
            TsdfFacilityEventId.New(), agg.TsdfFacilityId, agg.EpaFacilityId, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: RecordReceipt -----------------------------------------------------------

    /// <summary>Records receipt of a manifested shipment at this facility.</summary>
    public void RecordReceipt(Guid manifestId, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(TsdfFacilityStatus.Active);

        AddDomainEvent(new TsdfReceiptEvent(
            TsdfFacilityEventId.New(), TsdfFacilityId, manifestId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Suspend -----------------------------------------------------------

    /// <summary>Suspends the TSDF facility (e.g. permit expiry).</summary>
    public void Suspend(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(TsdfFacilityStatus.Active);

        Status = TsdfFacilityStatus.Suspended;

        AddDomainEvent(new TsdfFacilitySuspendedEvent(
            TsdfFacilityEventId.New(), TsdfFacilityId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ITsdfFacilityDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params TsdfFacilityStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for TsdfFacility {TsdfFacilityId}.");
    }
}
