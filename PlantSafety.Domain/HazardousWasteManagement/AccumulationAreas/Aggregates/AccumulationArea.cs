// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.AccumulationAreas
//  Layer:          Domain
//  File:           AccumulationArea.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a designated satellite or central hazardous-waste
//                  accumulation area at a site. Owns the default RCRA-style accumulation
//                  time-limit policy (90/180 days) applied to containers opened within it.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Enums;
using PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Events;
using PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Aggregates;

/// <summary>Aggregate root for a designated hazardous-waste accumulation area.</summary>
public sealed partial class AccumulationArea
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this AccumulationArea aggregate.</summary>
    public AccumulationAreaId AccumulationAreaId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public AccumulationAreaStatus Status { get; private set; }

    // ---- Area detail -----------------------------------------------------

    /// <summary>Human-readable name of the accumulation area.</summary>
    public string AreaName { get; private set; } = string.Empty;

    /// <summary>Physical location description within the site.</summary>
    public string LocationDescription { get; private set; } = string.Empty;

    /// <summary>Default RCRA-style accumulation time limit (in days) applied to containers opened here (90 for large-quantity generator satellite areas, 180/270 for central storage).</summary>
    public int TimeLimitDays { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IAccumulationAreaDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAccumulationAreaDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAccumulationAreaDomainEvent>(_domainEvents);

    // For rehydration only.
    private AccumulationArea() { }

    // ---- Factory: Create (Register) -----------------------------------------------------------

    /// <summary>Registers a new accumulation area and raises <see cref="AccumulationAreaRegisteredEvent"/>.</summary>
    public static AccumulationArea Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string areaName = "",
        string locationDescription = "",
        int timeLimitDays = 90)
    {
        var agg = new AccumulationArea
        {
            AccumulationAreaId    = AccumulationAreaId.New(),
            TenantId              = tenantId,
            SiteId                = siteId,
            Status                = AccumulationAreaStatus.Active,
            AreaName              = (areaName ?? string.Empty).Trim(),
            LocationDescription   = (locationDescription ?? string.Empty).Trim(),
            TimeLimitDays         = timeLimitDays,
            CreatedAtUtc          = createdAtUtc,
            CreatedBy             = createdBy,
        };

        agg.AddDomainEvent(new AccumulationAreaRegisteredEvent(
            AccumulationAreaEventId.New(), agg.AccumulationAreaId, timeLimitDays, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: FlagTimeLimitApproaching -----------------------------------------------

    /// <summary>Flags that a container within this area is approaching its regulatory time limit.</summary>
    public void FlagTimeLimitApproaching(
        Guid wasteContainerId, DateTime deadlineUtc,
        UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        Status = AccumulationAreaStatus.TimeLimitWarning;

        AddDomainEvent(new AccumulationTimeLimitApproachingEvent(
            AccumulationAreaEventId.New(), AccumulationAreaId, wasteContainerId, deadlineUtc,
            occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Deactivate -----------------------------------------------------

    /// <summary>Deactivates the accumulation area.</summary>
    public void Deactivate(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(AccumulationAreaStatus.Active, AccumulationAreaStatus.TimeLimitWarning);

        Status = AccumulationAreaStatus.Inactive;

        AddDomainEvent(new AccumulationAreaDeactivatedEvent(
            AccumulationAreaEventId.New(), AccumulationAreaId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IAccumulationAreaDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params AccumulationAreaStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for AccumulationArea {AccumulationAreaId}.");
    }
}
