// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.LoadSecurementChecks
//  Layer:          Domain
//  File:           LoadSecurementCheck.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single load securement check performed on a vehicle/trip prior
//                  to or during transport. Models the real lifecycle: recording (pass/fail outcome) and,
//                  when failed, corrective action completion.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Aggregates;

/// <summary>Aggregate root for a single load securement check.</summary>
public sealed partial class LoadSecurementCheck
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this LoadSecurementCheck aggregate.</summary>
    public LoadSecurementCheckId LoadSecurementCheckId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public LoadSecurementCheckStatus Status { get; private set; }

    // ---- Check identity ---------------------------------------------------

    /// <summary>Identity of the transport trip this check was performed for, if any.</summary>
    public Guid? TripId { get; private set; }

    /// <summary>Identity of the vehicle this check was performed on.</summary>
    public Guid VehicleId { get; private set; }

    /// <summary>Identity of the employee who performed the check.</summary>
    public Guid CheckedByUserId { get; private set; }

    /// <summary>UTC timestamp the check was performed.</summary>
    public UtcTimestamp CheckedAtUtc { get; private set; }

    /// <summary>Count of deficiencies observed during the check.</summary>
    public int DeficiencyCount { get; private set; }

    /// <summary>Securement score (0-100) recorded at check time.</summary>
    public decimal SecurementScore { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ILoadSecurementCheckDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ILoadSecurementCheckDomainEvent> DomainEvents
        => new ReadOnlyCollection<ILoadSecurementCheckDomainEvent>(_domainEvents);

    // For rehydration only.
    private LoadSecurementCheck() { }

    // ---- Factory: Create (Record) -----------------------------------------------------------

    /// <summary>Records a new load securement check and raises <see cref="LoadSecurementCheckRecordedEvent"/>
    /// (and additionally <see cref="LoadSecurementFailedEvent"/> when the outcome fails).</summary>
    public static LoadSecurementCheck Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid? tripId = null,
        Guid vehicleId = default,
        Guid checkedByUserId = default,
        bool passed = true,
        int deficiencyCount = 0,
        decimal securementScore = 100m)
    {
        if (securementScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(securementScore), "Securement score must be between 0 and 100.");

        var agg = new LoadSecurementCheck
        {
            LoadSecurementCheckId = LoadSecurementCheckId.New(),
            TenantId               = tenantId,
            SiteId                 = siteId,
            Status                 = passed ? LoadSecurementCheckStatus.Passed : LoadSecurementCheckStatus.Failed,
            TripId                 = tripId,
            VehicleId              = vehicleId,
            CheckedByUserId        = checkedByUserId,
            CheckedAtUtc           = createdAtUtc,
            DeficiencyCount        = deficiencyCount,
            SecurementScore        = securementScore,
            CreatedAtUtc           = createdAtUtc,
            CreatedBy              = createdBy,
        };

        agg.AddDomainEvent(new LoadSecurementCheckRecordedEvent(
            LoadSecurementCheckEventId.New(), agg.LoadSecurementCheckId, passed, createdAtUtc, actor, correlationId, causationId));

        if (!passed)
        {
            agg.AddDomainEvent(new LoadSecurementFailedEvent(
                LoadSecurementCheckEventId.New(), agg.LoadSecurementCheckId, deficiencyCount, createdAtUtc, actor, correlationId, causationId));
        }

        return agg;
    }

    // ---- Behavior: CompleteCorrectiveAction -----------------------------------------

    /// <summary>Marks corrective action for a failed check as completed.</summary>
    public void CompleteCorrectiveAction(UtcTimestamp completedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(LoadSecurementCheckStatus.Failed);

        Status = LoadSecurementCheckStatus.CorrectiveActionCompleted;

        AddDomainEvent(new LoadSecurementCorrectiveActionCompletedEvent(
            LoadSecurementCheckEventId.New(), LoadSecurementCheckId, completedAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ILoadSecurementCheckDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params LoadSecurementCheckStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new LoadSecurementCheckInvalidStatusTransitionException(LoadSecurementCheckId, Status.ToString(), "Requested", CorrelationId.New(), CausationId.New());
    }
}
