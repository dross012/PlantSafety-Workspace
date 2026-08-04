// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SensorIntegrityCheck
//  Layer:          Domain
//  File:           SensorIntegrityCheck.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the SensorIntegrityCheck bounded context (Sensor Integrity
//                  Monitoring subdomain of CyberPhysicalSecurity). Represents a single integrity
//                  verification run (calibration, signal, firmware, or physical-tamper check)
//                  against a sensor OtAsset. Supports the recurring sensor integrity check
//                  scheduling hook via NextCheckDueUtc.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Events;
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Aggregates;

/// <summary>Aggregate root for the SensorIntegrityCheck bounded context.</summary>
public sealed partial class SensorIntegrityCheck
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this SensorIntegrityCheck aggregate.</summary>
    public SensorIntegrityCheckId SensorIntegrityCheckId { get; private set; }

    /// <summary>Tenant that owns this check record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site the sensor is located at.</summary>
    public Guid SiteId { get; private set; }

    /// <summary>Identity of the sensor OT asset checked.</summary>
    public OtAssetId OtAssetId { get; private set; }

    // ---- Check details --------------------------------------------------------

    /// <summary>Category of integrity verification performed.</summary>
    public SensorIntegrityCheckType CheckType { get; private set; }

    /// <summary>Outcome of the check.</summary>
    public SensorIntegrityCheckResult Result { get; private set; }

    /// <summary>Free-form details of the check outcome (readings, deviation, evidence).</summary>
    public string Details { get; private set; } = string.Empty;

    /// <summary>UTC timestamp the check was performed.</summary>
    public UtcTimestamp CheckedAtUtc { get; private set; }

    /// <summary>UTC timestamp this sensor is next due for a recurring integrity check.</summary>
    public UtcTimestamp? NextCheckDueUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user/system that recorded this check (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<ISensorIntegrityCheckDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ISensorIntegrityCheckDomainEvent> DomainEvents
        => new ReadOnlyCollection<ISensorIntegrityCheckDomainEvent>(_domainEvents);

    // For rehydration only.
    private SensorIntegrityCheck() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Records a completed sensor-integrity check and raises the appropriate domain events.</summary>
    public static SensorIntegrityCheck Run(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid siteId,
        OtAssetId otAssetId,
        SensorIntegrityCheckType checkType,
        SensorIntegrityCheckResult result,
        string details,
        UtcTimestamp checkedAtUtc,
        UtcTimestamp? nextCheckDueUtc = null)
    {
        var agg = new SensorIntegrityCheck
        {
            SensorIntegrityCheckId = SensorIntegrityCheckId.New(),
            TenantId                = tenantId,
            SiteId                  = siteId,
            OtAssetId               = otAssetId,
            CheckType               = checkType,
            Result                  = result,
            Details                 = (details ?? string.Empty).Trim(),
            CheckedAtUtc            = checkedAtUtc,
            NextCheckDueUtc         = nextCheckDueUtc,
            CreatedAtUtc            = createdAtUtc,
            CreatedBy               = createdBy,
        };

        agg.AddDomainEvent(new SensorIntegrityCheckRunEvent(
            EventId:                 SensorIntegrityCheckEventId.New(),
            SensorIntegrityCheckId:  agg.SensorIntegrityCheckId,
            OtAssetId:               agg.OtAssetId,
            Result:                  result,
            CheckedAtUtc:            checkedAtUtc,
            Actor:                   actor,
            CorrelationId:           correlationId,
            CausationId:             causationId));

        if (result == SensorIntegrityCheckResult.Fail)
        {
            agg.AddDomainEvent(new SensorIntegrityCheckFailedEvent(
                EventId:                 SensorIntegrityCheckEventId.New(),
                SensorIntegrityCheckId:  agg.SensorIntegrityCheckId,
                OtAssetId:               agg.OtAssetId,
                Details:                 agg.Details,
                FailedAtUtc:             checkedAtUtc,
                Actor:                   actor,
                CorrelationId:           correlationId,
                CausationId:             causationId));
        }

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ISensorIntegrityCheckDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
