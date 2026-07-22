// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Domain
//  File:           HazardIsolationSequencing.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the HazardIsolationSequencing bounded context (module: Scheduling).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates;

/// <summary>Aggregate root for the HazardIsolationSequencing bounded context.</summary>
public sealed partial class HazardIsolationSequencing
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this HazardIsolationSequencing aggregate.</summary>
    public HazardIsolationSequencingId HazardIsolationSequencingId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public HazardIsolationSequencingStatus Status { get; private set; }

    // ---- HazardIsolationSequencing-specific business properties --------------------------------

    /// <summary>HazardZone business attribute.</summary>
    public string HazardZone { get; private set; }

    /// <summary>TaskType business attribute.</summary>
    public string TaskType { get; private set; }

    /// <summary>WindowStartUtc business attribute.</summary>
    public UtcTimestamp? WindowStartUtc { get; private set; }

    /// <summary>WindowEndUtc business attribute.</summary>
    public UtcTimestamp? WindowEndUtc { get; private set; }

    /// <summary>ConflictDetected business attribute.</summary>
    public bool ConflictDetected { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IHazardIsolationSequencingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IHazardIsolationSequencingDomainEvent> DomainEvents
        => new ReadOnlyCollection<IHazardIsolationSequencingDomainEvent>(_domainEvents);

    // For rehydration only.
    private HazardIsolationSequencing() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new HazardIsolationSequencing aggregate and raises <see cref="HazardIsolationSequencingCreatedEvent"/>.</summary>
    public static HazardIsolationSequencing Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string hazardZone = "",
        string taskType = "",
        UtcTimestamp? windowStartUtc = null,
        UtcTimestamp? windowEndUtc = null,
        bool conflictDetected = false)
    {
        var agg = new HazardIsolationSequencing
        {
            HazardIsolationSequencingId     = HazardIsolationSequencingId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = HazardIsolationSequencingStatus.Created,
            HazardZone = (hazardZone ?? string.Empty).Trim(),
            TaskType = (taskType ?? string.Empty).Trim(),
            WindowStartUtc = windowStartUtc,
            WindowEndUtc = windowEndUtc,
            ConflictDetected = conflictDetected,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new HazardIsolationSequencingCreatedEvent(
            EventId:       new HazardIsolationSequencingEventId(Uuid7.New()),
            HazardIsolationSequencingId:      agg.HazardIsolationSequencingId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IHazardIsolationSequencingDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params HazardIsolationSequencingStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for HazardIsolationSequencing {HazardIsolationSequencingId}.");
    }
}