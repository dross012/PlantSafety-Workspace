// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessAnomalies
//  Layer:          Domain
//  File:           AccessAnomaly.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single system-detected physical-access anomaly
//                  (tailgating, forced entry, after-hours access, restricted-zone violation,
//                  or unauthorized access). Feeds RiskManagement and auto-escalates via
//                  AlertEscalationPolicy for restricted-zone violations.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Aggregates;

/// <summary>Aggregate root for a single system-detected physical-access anomaly.</summary>
public sealed partial class AccessAnomaly
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this AccessAnomaly aggregate.</summary>
    public AccessAnomalyId AccessAnomalyId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current triage status.</summary>
    public AccessAnomalyStatus Status { get; private set; }

    // ---- Anomaly identity -----------------------------------------------------

    /// <summary>Identity of the AccessEvent that triggered this anomaly, if any.</summary>
    public Guid? AccessEventId { get; private set; }

    /// <summary>Identity of the AccessPoint where the anomaly occurred.</summary>
    public Guid AccessPointId { get; private set; }

    /// <summary>Identity of the Credential involved, if known.</summary>
    public Guid? CredentialId { get; private set; }

    /// <summary>Identity of the SafetyZone affected, if known.</summary>
    public Guid? SafetyZoneId { get; private set; }

    /// <summary>Classification of the anomaly.</summary>
    public AccessAnomalyType AnomalyType { get; private set; }

    /// <summary>Severity score (0-100) driving RiskManagement correlation and escalation urgency.</summary>
    public decimal SeverityScore { get; private set; }

    /// <summary>Identity of the user who acknowledged the anomaly, if any.</summary>
    public Guid? AcknowledgedByUserId { get; private set; }

    /// <summary>Resolution narrative recorded when the anomaly was resolved.</summary>
    public string? Resolution { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp (detection time).</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user/system actor who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IAccessAnomalyDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAccessAnomalyDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAccessAnomalyDomainEvent>(_domainEvents);

    // For rehydration only.
    private AccessAnomaly() { }

    // ---- Factory: Create (Detect) -----------------------------------------------------------

    /// <summary>Records a newly detected anomaly and raises the type-specific detection event.</summary>
    public static AccessAnomaly Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid? accessEventId = null,
        Guid accessPointId = default,
        Guid? credentialId = null,
        Guid? safetyZoneId = null,
        AccessAnomalyType anomalyType = AccessAnomalyType.UnauthorizedAccess,
        decimal severityScore = 50m)
    {
        if (severityScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(severityScore), "Severity score must be between 0 and 100.");

        var agg = new AccessAnomaly
        {
            AccessAnomalyId = AccessAnomalyId.New(),
            TenantId        = tenantId,
            SiteId          = siteId,
            Status          = AccessAnomalyStatus.Detected,
            AccessEventId   = accessEventId,
            AccessPointId   = accessPointId,
            CredentialId    = credentialId,
            SafetyZoneId    = safetyZoneId,
            AnomalyType     = anomalyType,
            SeverityScore   = severityScore,
            CreatedAtUtc    = createdAtUtc,
            CreatedBy       = createdBy,
        };

        var eventId = AccessAnomalyEventId.New();
        IAccessAnomalyDomainEvent detectionEvent = anomalyType switch
        {
            AccessAnomalyType.Tailgating => new TailgatingDetectedEvent(
                eventId, agg.AccessAnomalyId, accessPointId, credentialId, severityScore, createdAtUtc, actor, correlationId, causationId),
            AccessAnomalyType.AfterHoursAccess => new AfterHoursAccessDetectedEvent(
                eventId, agg.AccessAnomalyId, accessPointId, credentialId, severityScore, createdAtUtc, actor, correlationId, causationId),
            AccessAnomalyType.RestrictedZoneViolation => new RestrictedZoneViolationEvent(
                eventId, agg.AccessAnomalyId, accessPointId, credentialId, safetyZoneId ?? Guid.Empty, severityScore, createdAtUtc, actor, correlationId, causationId),
            _ => new UnauthorizedAccessDetectedEvent(
                eventId, agg.AccessAnomalyId, accessPointId, credentialId, severityScore, createdAtUtc, actor, correlationId, causationId),
        };

        agg.AddDomainEvent(detectionEvent);

        return agg;
    }

    // ---- Behavior: Acknowledge -----------------------------------------------

    /// <summary>Acknowledges the anomaly.</summary>
    public void Acknowledge(Guid acknowledgedByUserId, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(AccessAnomalyStatus.Detected);

        Status = AccessAnomalyStatus.Acknowledged;
        AcknowledgedByUserId = acknowledgedByUserId;

        AddDomainEvent(new AccessAnomalyAcknowledgedEvent(
            AccessAnomalyEventId.New(), AccessAnomalyId, acknowledgedByUserId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Escalate -----------------------------------------------------------

    /// <summary>Escalates the anomaly (e.g. via AlertEscalationPolicy).</summary>
    public void Escalate(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(AccessAnomalyStatus.Detected, AccessAnomalyStatus.Acknowledged);

        Status = AccessAnomalyStatus.Escalated;

        AddDomainEvent(new AccessAnomalyEscalatedEvent(
            AccessAnomalyEventId.New(), AccessAnomalyId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Resolve -----------------------------------------------------------

    /// <summary>Resolves the anomaly investigation.</summary>
    public void Resolve(string resolution, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(AccessAnomalyStatus.Acknowledged, AccessAnomalyStatus.Escalated);

        Status = AccessAnomalyStatus.Resolved;
        Resolution = (resolution ?? string.Empty).Trim();

        AddDomainEvent(new AccessAnomalyResolvedEvent(
            AccessAnomalyEventId.New(), AccessAnomalyId, Resolution, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IAccessAnomalyDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params AccessAnomalyStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for AccessAnomaly {AccessAnomalyId}.");
    }
}
