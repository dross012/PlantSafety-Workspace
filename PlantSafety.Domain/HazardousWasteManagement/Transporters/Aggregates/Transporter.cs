// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Transporters
//  Layer:          Domain
//  File:           Transporter.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for an EPA-registered hazardous-waste transporter. Integrates
//                  with Infrastructure.Integrations for transporter EDI (electronic manifest
//                  exchange) hand-off.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.Transporters.Enums;
using PlantSafety.Domain.HazardousWasteManagement.Transporters.Events;
using PlantSafety.Domain.HazardousWasteManagement.Transporters.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.Transporters.Aggregates;

/// <summary>Aggregate root for an EPA-registered hazardous-waste transporter.</summary>
public sealed partial class Transporter
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Transporter aggregate.</summary>
    public TransporterId TransporterId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public TransporterStatus Status { get; private set; }

    // ---- Transporter detail -----------------------------------------------------

    /// <summary>Legal company name.</summary>
    public string CompanyName { get; private set; } = string.Empty;

    /// <summary>EPA transporter identification number.</summary>
    public string EpaTransporterId { get; private set; } = string.Empty;

    /// <summary>UTC timestamp the transporter's operating license/permit expires.</summary>
    public UtcTimestamp? LicenseExpiresAtUtc { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ITransporterDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ITransporterDomainEvent> DomainEvents
        => new ReadOnlyCollection<ITransporterDomainEvent>(_domainEvents);

    // For rehydration only.
    private Transporter() { }

    // ---- Factory: Create (Register) -----------------------------------------------------------

    /// <summary>Registers a new transporter and raises <see cref="TransporterRegisteredEvent"/>.</summary>
    public static Transporter Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string companyName = "",
        string epaTransporterId = "",
        UtcTimestamp? licenseExpiresAtUtc = null)
    {
        var agg = new Transporter
        {
            TransporterId        = TransporterId.New(),
            TenantId              = tenantId,
            SiteId                = siteId,
            Status                = TransporterStatus.Active,
            CompanyName           = (companyName ?? string.Empty).Trim(),
            EpaTransporterId      = (epaTransporterId ?? string.Empty).Trim(),
            LicenseExpiresAtUtc   = licenseExpiresAtUtc,
            CreatedAtUtc          = createdAtUtc,
            CreatedBy             = createdBy,
        };

        agg.AddDomainEvent(new TransporterRegisteredEvent(
            TransporterEventId.New(), agg.TransporterId, agg.EpaTransporterId, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: RecordPickup -----------------------------------------------------------

    /// <summary>Records a pickup by this transporter against the given manifest.</summary>
    public void RecordPickup(Guid manifestId, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(TransporterStatus.Active);

        AddDomainEvent(new TransporterPickupEvent(
            TransporterEventId.New(), TransporterId, manifestId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Suspend -----------------------------------------------------------

    /// <summary>Suspends the transporter (e.g. license expiry).</summary>
    public void Suspend(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(TransporterStatus.Active);

        Status = TransporterStatus.Suspended;

        AddDomainEvent(new TransporterSuspendedEvent(
            TransporterEventId.New(), TransporterId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ITransporterDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params TransporterStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Transporter {TransporterId}.");
    }
}
