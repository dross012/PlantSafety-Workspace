// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteContainers
//  Layer:          Domain
//  File:           WasteContainer.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a physical hazardous-waste accumulation container within an
//                  AccumulationArea. Tracks the RCRA-style 90/180-day accumulation clock from the
//                  moment the container is opened until it is closed or shipped.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Enums;
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Events;
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Aggregates;

/// <summary>Aggregate root for a physical hazardous-waste accumulation container.</summary>
public sealed partial class WasteContainer
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this WasteContainer aggregate.</summary>
    public WasteContainerId WasteContainerId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public WasteContainerStatus Status { get; private set; }

    // ---- Container detail ----------------------------------------------------

    /// <summary>Identity of the parent AccumulationArea.</summary>
    public Guid AccumulationAreaId { get; private set; }

    /// <summary>Identity of the WasteStream accumulated in this container.</summary>
    public Guid WasteStreamId { get; private set; }

    /// <summary>Physical container label / barcode.</summary>
    public string ContainerLabel { get; private set; } = string.Empty;

    /// <summary>Rated capacity in gallons.</summary>
    public decimal CapacityGallons { get; private set; }

    /// <summary>Regulatory accumulation time limit in days (90 or 180 per RCRA generator status).</summary>
    public int RegulatoryTimeLimitDays { get; private set; }

    /// <summary>UTC timestamp the container was opened (accumulation clock start).</summary>
    public UtcTimestamp OpenedAtUtc { get; private set; }

    /// <summary>UTC timestamp the container was closed, if applicable.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IWasteContainerDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IWasteContainerDomainEvent> DomainEvents
        => new ReadOnlyCollection<IWasteContainerDomainEvent>(_domainEvents);

    // For rehydration only.
    private WasteContainer() { }

    // ---- Factory: Create (Open) -----------------------------------------------------------

    /// <summary>Opens a new accumulation container and raises <see cref="AccumulationContainerOpenedEvent"/>.</summary>
    public static WasteContainer Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid accumulationAreaId = default,
        Guid wasteStreamId = default,
        string containerLabel = "",
        decimal capacityGallons = 0m,
        int regulatoryTimeLimitDays = 90)
    {
        var agg = new WasteContainer
        {
            WasteContainerId         = WasteContainerId.New(),
            TenantId                 = tenantId,
            SiteId                   = siteId,
            Status                   = WasteContainerStatus.Open,
            AccumulationAreaId       = accumulationAreaId,
            WasteStreamId            = wasteStreamId,
            ContainerLabel           = (containerLabel ?? string.Empty).Trim(),
            CapacityGallons          = capacityGallons,
            RegulatoryTimeLimitDays  = regulatoryTimeLimitDays,
            OpenedAtUtc              = createdAtUtc,
            CreatedAtUtc             = createdAtUtc,
            CreatedBy                = createdBy,
        };

        agg.AddDomainEvent(new AccumulationContainerOpenedEvent(
            WasteContainerEventId.New(), agg.WasteContainerId, accumulationAreaId, wasteStreamId,
            createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: Close -----------------------------------------------------------

    /// <summary>Closes the accumulation container.</summary>
    public void Close(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(WasteContainerStatus.Open);

        Status = WasteContainerStatus.Closed;
        ClosedAtUtc = occurredAtUtc;

        AddDomainEvent(new AccumulationContainerClosedEvent(
            WasteContainerEventId.New(), WasteContainerId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: MarkShipped -----------------------------------------------------------

    /// <summary>Marks the container as shipped off-site under the given manifest.</summary>
    public void MarkShipped(Guid manifestId, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(WasteContainerStatus.Closed);

        Status = WasteContainerStatus.Shipped;

        AddDomainEvent(new WasteContainerShippedEvent(
            WasteContainerEventId.New(), WasteContainerId, manifestId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Query helpers -------------------------------------------------------------

    /// <summary>Returns the UTC instant this container breaches its regulatory accumulation time limit.</summary>
    public DateTime TimeLimitDeadlineUtc() => OpenedAtUtc.Value.AddDays(RegulatoryTimeLimitDays);

    /// <summary>Returns <see langword="true"/> when the accumulation clock has exceeded the regulatory time limit.</summary>
    public bool IsPastTimeLimit(DateTime nowUtc) => Status == WasteContainerStatus.Open && nowUtc > TimeLimitDeadlineUtc();

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IWasteContainerDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params WasteContainerStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for WasteContainer {WasteContainerId}.");
    }
}
