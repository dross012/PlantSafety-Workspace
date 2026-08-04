// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Domain
//  File:           AutonomousVehicleAsset.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single autonomous/semi-autonomous vehicle or mobile-robot
//                  asset (AGV, AMR, autonomous yard truck) operating on-site. Models registration,
//                  autonomy-mode transitions, suspension of autonomous operation, reactivation, and
//                  retirement. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Aggregates;

/// <summary>Aggregate root for a single autonomous/semi-autonomous vehicle or mobile-robot asset.</summary>
public sealed partial class AutonomousVehicleAsset
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this AutonomousVehicleAsset aggregate.</summary>
    public AutonomousVehicleAssetId AutonomousVehicleAssetId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public AutonomousVehicleAssetStatus Status { get; private set; }

    // ---- Asset identity --------------------------------------------------------

    /// <summary>Human-readable asset tag/title.</summary>
    public string AssetTag { get; private set; } = string.Empty;

    /// <summary>Vehicle/mobile-robot classification.</summary>
    public AutonomousVehicleType VehicleType { get; private set; }

    /// <summary>Rated degree of autonomy.</summary>
    public AutonomyLevel AutonomyLevel { get; private set; }

    /// <summary>Current operating mode.</summary>
    public AutonomyMode CurrentAutonomyMode { get; private set; }

    /// <summary>Identity of the currently linked safety-case validation, if any.</summary>
    public Guid? SafetyCaseValidationId { get; private set; }

    /// <summary>Cumulative count of recorded autonomy-mode transitions.</summary>
    public int ModeTransitionCount { get; private set; }

    /// <summary>Reason recorded for the most recent suspension, if any.</summary>
    public string? SuspensionReason { get; private set; }

    /// <summary>UTC timestamp of the most recent suspension, if any.</summary>
    public UtcTimestamp? SuspendedAtUtc { get; private set; }

    // ---- Audit -------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -------------------------------------------------------

    private readonly List<IAutonomousVehicleAssetDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAutonomousVehicleAssetDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAutonomousVehicleAssetDomainEvent>(_domainEvents);

    // For rehydration only.
    private AutonomousVehicleAsset() { }

    // ---- Factory: Create (Register) -------------------------------------------

    /// <summary>Registers a new autonomous vehicle asset and raises <see cref="AutonomousVehicleAssetRegisteredEvent"/>.</summary>
    public static AutonomousVehicleAsset Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string assetTag = "",
        AutonomousVehicleType vehicleType = AutonomousVehicleType.Amr,
        AutonomyLevel autonomyLevel = AutonomyLevel.SemiAutonomous)
    {
        var agg = new AutonomousVehicleAsset
        {
            AutonomousVehicleAssetId = AutonomousVehicleAssetId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = AutonomousVehicleAssetStatus.Registered,
            AssetTag = (assetTag ?? string.Empty).Trim(),
            VehicleType = vehicleType,
            AutonomyLevel = autonomyLevel,
            CurrentAutonomyMode = AutonomyMode.Manual,
            ModeTransitionCount = 0,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new AutonomousVehicleAssetRegisteredEvent(
            AutonomousVehicleAssetEventId.New(), agg.AutonomousVehicleAssetId, vehicleType, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: RecordModeTransition ---------------------------------------

    /// <summary>Records an autonomy-mode transition (e.g. Manual to Autonomous).</summary>
    public void RecordModeTransition(AutonomyMode toMode, UtcTimestamp transitionedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(AutonomousVehicleAssetStatus.Registered, AutonomousVehicleAssetStatus.Active);

        var fromMode = CurrentAutonomyMode;
        CurrentAutonomyMode = toMode;
        ModeTransitionCount++;

        if (Status == AutonomousVehicleAssetStatus.Registered)
            Status = AutonomousVehicleAssetStatus.Active;

        AddDomainEvent(new AutonomyModeTransitionedEvent(
            AutonomousVehicleAssetEventId.New(), AutonomousVehicleAssetId, fromMode, toMode, transitionedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Suspend -----------------------------------------------------

    /// <summary>Suspends autonomous operation of the asset pending review.</summary>
    public void Suspend(string reason, UtcTimestamp suspendedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(AutonomousVehicleAssetStatus.Active);

        Status = AutonomousVehicleAssetStatus.Suspended;
        SuspensionReason = (reason ?? string.Empty).Trim();
        SuspendedAtUtc = suspendedAtUtc;

        AddDomainEvent(new AutonomousOperationSuspendedEvent(
            AutonomousVehicleAssetEventId.New(), AutonomousVehicleAssetId, SuspensionReason, suspendedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Reactivate ---------------------------------------------------

    /// <summary>Reactivates a previously suspended asset.</summary>
    public void Reactivate(UtcTimestamp reactivatedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(AutonomousVehicleAssetStatus.Suspended);

        Status = AutonomousVehicleAssetStatus.Active;
        SuspensionReason = null;

        AddDomainEvent(new AutonomousVehicleAssetReactivatedEvent(
            AutonomousVehicleAssetEventId.New(), AutonomousVehicleAssetId, reactivatedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Retire -------------------------------------------------------

    /// <summary>Retires the asset.</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == AutonomousVehicleAssetStatus.Retired)
            throw new AutonomousVehicleAssetInvalidStatusTransitionException(AutonomousVehicleAssetId, Status.ToString(), AutonomousVehicleAssetStatus.Retired.ToString(), correlationId, causationId);

        Status = AutonomousVehicleAssetStatus.Retired;

        AddDomainEvent(new AutonomousVehicleAssetRetiredEvent(
            AutonomousVehicleAssetEventId.New(), AutonomousVehicleAssetId, retiredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IAutonomousVehicleAssetDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params AutonomousVehicleAssetStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for AutonomousVehicleAsset {AutonomousVehicleAssetId}.");
    }
}
