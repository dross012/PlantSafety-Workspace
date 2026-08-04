// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessEvents
//  Layer:          Domain
//  File:           AccessEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single physical-access event ingested from an
//                  external badge-system webhook or IoT feed (Infrastructure.Integrations).
//                  Raw access events are retained ~1 year, after which they are aggregated/
//                  anonymized per data-governance policy.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates;

/// <summary>Aggregate root for a single physical-access event.</summary>
public sealed partial class AccessEvent
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this AccessEvent aggregate.</summary>
    public AccessEventId AccessEventId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current processing status.</summary>
    public AccessEventStatus Status { get; private set; }

    // ---- Event identity -----------------------------------------------------

    /// <summary>Identity of the AccessPoint that produced this event.</summary>
    public Guid AccessPointId { get; private set; }

    /// <summary>Identity of the Credential presented, or null (denied/forced-entry with no valid badge).</summary>
    public Guid? CredentialId { get; private set; }

    /// <summary>Identity of the SafetyZone this event affects, if known.</summary>
    public Guid? SafetyZoneId { get; private set; }

    /// <summary>Raw event outcome as reported by the badge system.</summary>
    public AccessEventType EventType { get; private set; }

    /// <summary>Direction of travel (for occupancy correlation).</summary>
    public AccessEventDirection Direction { get; private set; }

    /// <summary>UTC timestamp the underlying physical event occurred (as reported by the badge system).</summary>
    public UtcTimestamp OccurredAtUtc { get; private set; }

    /// <summary>Anomaly type recorded when this event was flagged, if any.</summary>
    public string? FlaggedAnomalyType { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp (ingestion time).</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user/system actor who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IAccessEventDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAccessEventDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAccessEventDomainEvent>(_domainEvents);

    // For rehydration only.
    private AccessEvent() { }

    // ---- Factory: Create (Ingest) -----------------------------------------------------------

    /// <summary>Ingests a new raw access event and raises <see cref="AccessEventIngestedEvent"/>.</summary>
    public static AccessEvent Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid accessPointId = default,
        Guid? credentialId = null,
        Guid? safetyZoneId = null,
        AccessEventType eventType = AccessEventType.Granted,
        AccessEventDirection direction = AccessEventDirection.In,
        UtcTimestamp? occurredAtUtc = null)
    {
        var effectiveOccurredAt = occurredAtUtc ?? createdAtUtc;

        var agg = new AccessEvent
        {
            AccessEventId  = AccessEventId.New(),
            TenantId       = tenantId,
            SiteId         = siteId,
            Status         = AccessEventStatus.Ingested,
            AccessPointId  = accessPointId,
            CredentialId   = credentialId,
            SafetyZoneId   = safetyZoneId,
            EventType      = eventType,
            Direction      = direction,
            OccurredAtUtc  = effectiveOccurredAt,
            CreatedAtUtc   = createdAtUtc,
            CreatedBy      = createdBy,
        };

        agg.AddDomainEvent(new AccessEventIngestedEvent(
            AccessEventEventId.New(), agg.AccessEventId, accessPointId, credentialId, eventType, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: MarkProcessed -----------------------------------------------

    /// <summary>Marks the event as processed (occupancy/anomaly correlation complete, no anomaly found).</summary>
    public void MarkProcessed(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(AccessEventStatus.Ingested);

        Status = AccessEventStatus.Processed;

        AddDomainEvent(new AccessEventProcessedEvent(
            AccessEventEventId.New(), AccessEventId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: FlagAnomaly -----------------------------------------------------------

    /// <summary>Flags the event as anomalous.</summary>
    public void FlagAnomaly(string anomalyType, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(AccessEventStatus.Ingested, AccessEventStatus.Processed);

        Status = AccessEventStatus.Flagged;
        FlaggedAnomalyType = (anomalyType ?? string.Empty).Trim();

        AddDomainEvent(new AccessEventFlaggedEvent(
            AccessEventEventId.New(), AccessEventId, FlaggedAnomalyType, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IAccessEventDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params AccessEventStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for AccessEvent {AccessEventId}.");
    }
}
