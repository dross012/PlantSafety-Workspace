// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitScheduling
//  Layer:          Domain
//  File:           PermitScheduling.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the PermitScheduling bounded context (module: PermitToWork).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates;

/// <summary>Aggregate root for the PermitScheduling bounded context.</summary>
public sealed partial class PermitScheduling
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this PermitScheduling aggregate.</summary>
    public PermitSchedulingId PermitSchedulingId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PermitSchedulingStatus Status { get; private set; }

    // ---- PermitScheduling-specific business properties --------------------------------

    /// <summary>PermitNumber business attribute.</summary>
    public string PermitNumber { get; private set; }

    /// <summary>ScheduledStartUtc business attribute.</summary>
    public UtcTimestamp? ScheduledStartUtc { get; private set; }

    /// <summary>ScheduledEndUtc business attribute.</summary>
    public UtcTimestamp? ScheduledEndUtc { get; private set; }

    /// <summary>ConflictDetected business attribute.</summary>
    public bool ConflictDetected { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPermitSchedulingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPermitSchedulingDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPermitSchedulingDomainEvent>(_domainEvents);

    // For rehydration only.
    private PermitScheduling() 
    {
        // Assigned by the data mapper immediately after construction; see the public
        // constructor for the invariant these properties actually hold.
        PermitNumber = null!;
    }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new PermitScheduling aggregate and raises <see cref="PermitSchedulingCreatedEvent"/>.</summary>
    public static PermitScheduling Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string permitNumber = "",
        UtcTimestamp? scheduledStartUtc = null,
        UtcTimestamp? scheduledEndUtc = null,
        bool conflictDetected = false)
    {
        var agg = new PermitScheduling
        {
            PermitSchedulingId     = PermitSchedulingId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = PermitSchedulingStatus.Created,
            PermitNumber = (permitNumber ?? string.Empty).Trim(),
            ScheduledStartUtc = scheduledStartUtc,
            ScheduledEndUtc = scheduledEndUtc,
            ConflictDetected = conflictDetected,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new PermitSchedulingCreatedEvent(
            EventId:       new PermitSchedulingEventId(Uuid7.New()),
            PermitSchedulingId:      agg.PermitSchedulingId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPermitSchedulingDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PermitSchedulingStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for PermitScheduling {PermitSchedulingId}.");
    }
}