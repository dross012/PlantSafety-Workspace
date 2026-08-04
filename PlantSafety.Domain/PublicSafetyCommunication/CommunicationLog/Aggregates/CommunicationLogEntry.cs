// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.CommunicationLog
//  Layer:          Domain
//  File:           CommunicationLogEntry.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single logged communication with an external
//                  public-safety stakeholder (fire, police, EMS, regulatory hotline, public alert
//                  authority), covering both notification dispatch/broadcast records and free-form
//                  after-action communication entries. Data-governance policy: this aggregate is
//                  immutable once recorded -- no lifecycle mutation methods are exposed (see
//                  CommunicationLogImmutabilityRules) -- and is retained per the incident-record
//                  retention policy as compliance evidence supporting the After-Action
//                  Communication Timeline dashboard.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Aggregates;

/// <summary>Aggregate root for a single, immutable logged communication with an external public-safety stakeholder.</summary>
public sealed partial class CommunicationLogEntry
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this CommunicationLogEntry aggregate.</summary>
    public CommunicationLogEntryId CommunicationLogEntryId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    // ---- Correlation -----------------------------------------------------------

    /// <summary>Identity of the originating SafetyEvents.EmergencyManagement incident, if any.</summary>
    public Guid? IncidentId { get; private set; }

    /// <summary>Identity of the related IncidentNotification, if this entry documents a notification.</summary>
    public Guid? RelatedIncidentNotificationId { get; private set; }

    /// <summary>Identity of the related PublicAlertBroadcast, if this entry documents a broadcast.</summary>
    public Guid? RelatedPublicAlertBroadcastId { get; private set; }

    // ---- Content -----------------------------------------------------------------

    /// <summary>Direction of the communication (Outbound/Inbound).</summary>
    public CommunicationDirection Direction { get; private set; }

    /// <summary>Medium used for the communication.</summary>
    public CommunicationMedium Medium { get; private set; }

    /// <summary>Name of the external counterparty (agency/contact) involved.</summary>
    public string CounterpartyName { get; private set; } = string.Empty;

    /// <summary>Free-text summary of the communication content.</summary>
    public string Summary { get; private set; } = string.Empty;

    /// <summary>UTC timestamp the communication actually occurred.</summary>
    public UtcTimestamp OccurredAtUtc { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC timestamp this record was logged (immutable once written).</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who logged this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ICommunicationLogEntryDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ICommunicationLogEntryDomainEvent> DomainEvents
        => new ReadOnlyCollection<ICommunicationLogEntryDomainEvent>(_domainEvents);

    // For rehydration only.
    private CommunicationLogEntry() { }

    // ---- Factory: Create (Record) -----------------------------------------------------------

    /// <summary>Records a new, immutable communication log entry and raises <see cref="CommunicationLogEntryRecordedEvent"/>.</summary>
    public static CommunicationLogEntry Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        Guid? incidentId,
        Guid? relatedIncidentNotificationId,
        Guid? relatedPublicAlertBroadcastId,
        CommunicationDirection direction,
        CommunicationMedium medium,
        string counterpartyName,
        string summary,
        UtcTimestamp occurredAtUtc)
    {
        var agg = new CommunicationLogEntry
        {
            CommunicationLogEntryId       = CommunicationLogEntryId.New(),
            TenantId                       = tenantId,
            SiteId                         = siteId,
            IncidentId                     = incidentId,
            RelatedIncidentNotificationId  = relatedIncidentNotificationId,
            RelatedPublicAlertBroadcastId  = relatedPublicAlertBroadcastId,
            Direction                      = direction,
            Medium                         = medium,
            CounterpartyName               = (counterpartyName ?? string.Empty).Trim(),
            Summary                        = (summary ?? string.Empty).Trim(),
            OccurredAtUtc                  = occurredAtUtc,
            CreatedAtUtc                   = createdAtUtc,
            CreatedBy                      = createdBy,
        };

        agg.AddDomainEvent(new CommunicationLogEntryRecordedEvent(
            CommunicationLogEntryEventId.New(), agg.CommunicationLogEntryId, incidentId, direction.ToString(), medium.ToString(), createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ICommunicationLogEntryDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
