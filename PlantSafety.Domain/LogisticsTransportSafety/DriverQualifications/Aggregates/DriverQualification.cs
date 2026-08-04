// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.DriverQualifications
//  Layer:          Domain
//  File:           DriverQualification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single driver's transport safety qualification/credential
//                  (CDL, hazmat endorsement, forklift certification, defensive driving). Models the
//                  real lifecycle: qualification, suspension, reinstatement, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Aggregates;

/// <summary>Aggregate root for a single driver's transport safety qualification.</summary>
public sealed partial class DriverQualification
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this DriverQualification aggregate.</summary>
    public DriverQualificationId DriverQualificationId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public DriverQualificationStatus Status { get; private set; }

    // ---- Qualification identity ---------------------------------------------------

    /// <summary>Identity of the employee/driver this qualification belongs to.</summary>
    public Guid DriverUserId { get; private set; }

    /// <summary>Qualification/credential type.</summary>
    public DriverQualificationType QualificationType { get; private set; }

    /// <summary>License or credential number.</summary>
    public string LicenseNumber { get; private set; } = string.Empty;

    /// <summary>UTC timestamp the credential was issued.</summary>
    public UtcTimestamp IssuedAtUtc { get; private set; }

    /// <summary>UTC timestamp the credential expires.</summary>
    public UtcTimestamp ExpiresAtUtc { get; private set; }

    /// <summary>Reason recorded for the most recent suspension, if any.</summary>
    public string? SuspensionReason { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IDriverQualificationDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IDriverQualificationDomainEvent> DomainEvents
        => new ReadOnlyCollection<IDriverQualificationDomainEvent>(_domainEvents);

    // For rehydration only.
    private DriverQualification() { }

    // ---- Factory: Create (Qualify) -----------------------------------------------------------

    /// <summary>Qualifies a new driver and raises <see cref="DriverQualifiedEvent"/>.</summary>
    public static DriverQualification Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid driverUserId = default,
        DriverQualificationType qualificationType = DriverQualificationType.CommercialDriverLicense,
        string licenseNumber = "",
        UtcTimestamp issuedAtUtc = default,
        UtcTimestamp expiresAtUtc = default)
    {
        var agg = new DriverQualification
        {
            DriverQualificationId = DriverQualificationId.New(),
            TenantId               = tenantId,
            SiteId                 = siteId,
            Status                 = DriverQualificationStatus.Qualified,
            DriverUserId           = driverUserId,
            QualificationType      = qualificationType,
            LicenseNumber          = (licenseNumber ?? string.Empty).Trim(),
            IssuedAtUtc            = issuedAtUtc,
            ExpiresAtUtc           = expiresAtUtc,
            CreatedAtUtc           = createdAtUtc,
            CreatedBy              = createdBy,
        };

        agg.AddDomainEvent(new DriverQualifiedEvent(
            DriverQualificationEventId.New(), agg.DriverQualificationId, driverUserId, qualificationType, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: Suspend -----------------------------------------

    /// <summary>Suspends this driver's qualification.</summary>
    public void Suspend(string reason, UtcTimestamp suspendedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(DriverQualificationStatus.Qualified);

        SuspensionReason = (reason ?? string.Empty).Trim();
        Status = DriverQualificationStatus.Suspended;

        AddDomainEvent(new DriverQualificationSuspendedEvent(
            DriverQualificationEventId.New(), DriverQualificationId, suspendedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Reinstate -----------------------------------------

    /// <summary>Reinstates this driver's suspended qualification.</summary>
    public void Reinstate(UtcTimestamp reinstatedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(DriverQualificationStatus.Suspended);

        SuspensionReason = null;
        Status = DriverQualificationStatus.Qualified;

        AddDomainEvent(new DriverQualificationReinstatedEvent(
            DriverQualificationEventId.New(), DriverQualificationId, reinstatedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Retire -----------------------------------------------------------

    /// <summary>Retires this driver's qualification.</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == DriverQualificationStatus.Retired)
            throw new DriverQualificationInvalidStatusTransitionException(DriverQualificationId, Status.ToString(), DriverQualificationStatus.Retired.ToString(), correlationId, causationId);

        Status = DriverQualificationStatus.Retired;

        AddDomainEvent(new DriverQualificationRetiredEvent(
            DriverQualificationEventId.New(), DriverQualificationId, retiredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IDriverQualificationDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params DriverQualificationStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for DriverQualification {DriverQualificationId}.");
    }
}
