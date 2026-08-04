// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportVehicles
//  Layer:          Domain
//  File:           TransportVehicle.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single on-site material/personnel transport vehicle (truck,
//                  forklift, trailer, van, heavy equipment carrier). Models the real lifecycle:
//                  registration, scheduled safety inspection, pass/fail outcome, out-of-service and
//                  retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Aggregates;

/// <summary>Aggregate root for a single transport vehicle.</summary>
public sealed partial class TransportVehicle
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this TransportVehicle aggregate.</summary>
    public TransportVehicleId TransportVehicleId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public TransportVehicleStatus Status { get; private set; }

    // ---- Vehicle identity ---------------------------------------------------

    /// <summary>Human-readable title of this vehicle.</summary>
    public string VehicleTitle { get; private set; } = string.Empty;

    /// <summary>Vehicle type classification.</summary>
    public TransportVehicleType VehicleType { get; private set; }

    /// <summary>Plate number or internal asset tag identifying this vehicle.</summary>
    public string VehicleIdentifier { get; private set; } = string.Empty;

    /// <summary>UTC timestamp of the most recent inspection, if any.</summary>
    public UtcTimestamp? LastInspectionAtUtc { get; private set; }

    /// <summary>UTC timestamp the next inspection is due.</summary>
    public UtcTimestamp? NextInspectionDueAtUtc { get; private set; }

    /// <summary>Inspection score (0-100) from the most recent inspection.</summary>
    public decimal? LastInspectionScore { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ITransportVehicleDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ITransportVehicleDomainEvent> DomainEvents
        => new ReadOnlyCollection<ITransportVehicleDomainEvent>(_domainEvents);

    // For rehydration only.
    private TransportVehicle() { }

    // ---- Factory: Create (Register) -----------------------------------------------------------

    /// <summary>Registers a new transport vehicle and raises <see cref="TransportVehicleRegisteredEvent"/>.</summary>
    public static TransportVehicle Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string vehicleTitle = "",
        TransportVehicleType vehicleType = TransportVehicleType.Truck,
        string vehicleIdentifier = "")
    {
        var agg = new TransportVehicle
        {
            TransportVehicleId = TransportVehicleId.New(),
            TenantId           = tenantId,
            SiteId             = siteId,
            Status             = TransportVehicleStatus.Registered,
            VehicleTitle       = (vehicleTitle ?? string.Empty).Trim(),
            VehicleType        = vehicleType,
            VehicleIdentifier  = (vehicleIdentifier ?? string.Empty).Trim(),
            CreatedAtUtc       = createdAtUtc,
            CreatedBy          = createdBy,
        };

        agg.AddDomainEvent(new TransportVehicleRegisteredEvent(
            TransportVehicleEventId.New(), agg.TransportVehicleId, vehicleType, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: ScheduleInspection -----------------------------------------

    /// <summary>Schedules the next safety inspection.</summary>
    public void ScheduleInspection(UtcTimestamp dueAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(TransportVehicleStatus.Registered, TransportVehicleStatus.InspectionPassed, TransportVehicleStatus.InspectionFailed);

        NextInspectionDueAtUtc = dueAtUtc;
        Status = TransportVehicleStatus.InspectionScheduled;

        AddDomainEvent(new TransportVehicleInspectionScheduledEvent(
            TransportVehicleEventId.New(), TransportVehicleId, dueAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: RecordInspectionResult -----------------------------------------

    /// <summary>Records the outcome of a scheduled safety inspection.</summary>
    public void RecordInspectionResult(bool passed, decimal inspectionScore, UtcTimestamp inspectedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(TransportVehicleStatus.InspectionScheduled);

        if (inspectionScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(inspectionScore), "Inspection score must be between 0 and 100.");

        LastInspectionAtUtc  = inspectedAtUtc;
        LastInspectionScore  = inspectionScore;
        Status = passed ? TransportVehicleStatus.InspectionPassed : TransportVehicleStatus.InspectionFailed;

        if (!passed)
        {
            Status = TransportVehicleStatus.OutOfService;
        }

        AddDomainEvent(new TransportVehicleInspectionRecordedEvent(
            TransportVehicleEventId.New(), TransportVehicleId, passed, inspectionScore, inspectedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Retire -----------------------------------------------------------

    /// <summary>Retires this vehicle.</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == TransportVehicleStatus.Retired)
            throw new TransportVehicleInvalidStatusTransitionException(TransportVehicleId, Status.ToString(), TransportVehicleStatus.Retired.ToString(), correlationId, causationId);

        Status = TransportVehicleStatus.Retired;

        AddDomainEvent(new TransportVehicleRetiredEvent(
            TransportVehicleEventId.New(), TransportVehicleId, retiredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ITransportVehicleDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params TransportVehicleStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for TransportVehicle {TransportVehicleId}.");
    }
}
