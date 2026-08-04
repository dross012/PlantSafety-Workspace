// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.Carriers
//  Layer:          Domain
//  File:           Carrier.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single fleet or third-party transport carrier. Models the real
//                  lifecycle: registration, safety/insurance qualification, suspension, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.Carriers.Aggregates;

/// <summary>Aggregate root for a single fleet or third-party transport carrier.</summary>
public sealed partial class Carrier
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Carrier aggregate.</summary>
    public CarrierId CarrierId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public CarrierStatus Status { get; private set; }

    // ---- Carrier identity ---------------------------------------------------

    /// <summary>Legal or trading name of the carrier.</summary>
    public string CarrierName { get; private set; } = string.Empty;

    /// <summary>Carrier relationship type.</summary>
    public CarrierType CarrierType { get; private set; }

    /// <summary>UTC timestamp when the carrier's insurance coverage expires.</summary>
    public UtcTimestamp? InsuranceExpiryAtUtc { get; private set; }

    /// <summary>Safety rating score (0-100) from the most recent qualification review.</summary>
    public decimal? SafetyRatingScore { get; private set; }

    /// <summary>UTC timestamp when the carrier was most recently qualified.</summary>
    public UtcTimestamp? QualifiedAtUtc { get; private set; }

    /// <summary>Reason recorded for the most recent suspension, if any.</summary>
    public string? SuspensionReason { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ICarrierDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ICarrierDomainEvent> DomainEvents
        => new ReadOnlyCollection<ICarrierDomainEvent>(_domainEvents);

    // For rehydration only.
    private Carrier() { }

    // ---- Factory: Create (Register) -----------------------------------------------------------

    /// <summary>Registers a new carrier and raises <see cref="CarrierRegisteredEvent"/>.</summary>
    public static Carrier Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string carrierName = "",
        CarrierType carrierType = CarrierType.InHouseFleet,
        UtcTimestamp? insuranceExpiryAtUtc = null)
    {
        var agg = new Carrier
        {
            CarrierId             = CarrierId.New(),
            TenantId               = tenantId,
            SiteId                 = siteId,
            Status                 = CarrierStatus.PendingQualification,
            CarrierName            = (carrierName ?? string.Empty).Trim(),
            CarrierType            = carrierType,
            InsuranceExpiryAtUtc   = insuranceExpiryAtUtc,
            CreatedAtUtc           = createdAtUtc,
            CreatedBy              = createdBy,
        };

        agg.AddDomainEvent(new CarrierRegisteredEvent(
            CarrierEventId.New(), agg.CarrierId, carrierType, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: Qualify -----------------------------------------

    /// <summary>Qualifies this carrier to perform transport work.</summary>
    public void Qualify(decimal safetyRatingScore, UtcTimestamp qualifiedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(CarrierStatus.PendingQualification, CarrierStatus.Suspended);

        if (safetyRatingScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(safetyRatingScore), "Safety rating score must be between 0 and 100.");

        SafetyRatingScore = safetyRatingScore;
        QualifiedAtUtc     = qualifiedAtUtc;
        SuspensionReason   = null;
        Status = CarrierStatus.Qualified;

        AddDomainEvent(new CarrierQualifiedEvent(
            CarrierEventId.New(), CarrierId, safetyRatingScore, qualifiedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Suspend -----------------------------------------

    /// <summary>Suspends this carrier from performing transport work.</summary>
    public void Suspend(string reason, UtcTimestamp suspendedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(CarrierStatus.Qualified);

        SuspensionReason = (reason ?? string.Empty).Trim();
        Status = CarrierStatus.Suspended;

        AddDomainEvent(new CarrierSuspendedEvent(
            CarrierEventId.New(), CarrierId, suspendedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Retire -----------------------------------------------------------

    /// <summary>Retires this carrier.</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == CarrierStatus.Retired)
            throw new CarrierInvalidStatusTransitionException(CarrierId, Status.ToString(), CarrierStatus.Retired.ToString(), correlationId, causationId);

        Status = CarrierStatus.Retired;

        AddDomainEvent(new CarrierRetiredEvent(
            CarrierEventId.New(), CarrierId, retiredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ICarrierDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params CarrierStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Carrier {CarrierId}.");
    }
}
