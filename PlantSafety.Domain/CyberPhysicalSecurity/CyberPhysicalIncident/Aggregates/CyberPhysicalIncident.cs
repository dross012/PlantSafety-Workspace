// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.CyberPhysicalIncident
//  Layer:          Domain
//  File:           CyberPhysicalIncident.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the CyberPhysicalIncident bounded context (Incident Response
//                  subdomain of CyberPhysicalSecurity). Represents a confirmed or escalated
//                  cyber-physical security event requiring a coordinated response, typically opened
//                  from one or more confirmed/escalated SabotageIndicators.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Events;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Exceptions;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Aggregates;

/// <summary>Aggregate root for the CyberPhysicalIncident bounded context.</summary>
public sealed partial class CyberPhysicalIncident
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this CyberPhysicalIncident aggregate.</summary>
    public CyberPhysicalIncidentId CyberPhysicalIncidentId { get; private set; }

    /// <summary>Tenant that owns this incident.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site the incident relates to.</summary>
    public Guid SiteId { get; private set; }

    // ---- Incident details --------------------------------------------------------

    /// <summary>Short incident title.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Full incident description/narrative.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Severity rating.</summary>
    public CyberPhysicalIncidentSeverity Severity { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public CyberPhysicalIncidentStatus Status { get; private set; }

    /// <summary>Identities of the SabotageIndicators that contributed to opening this incident.</summary>
    public IReadOnlyList<Guid> RelatedSabotageIndicatorIds => _relatedSabotageIndicatorIds.AsReadOnly();
    private readonly List<Guid> _relatedSabotageIndicatorIds = [];

    /// <summary>Identity of the accountable incident owner/responder.</summary>
    public UserId OwnerId { get; private set; }

    /// <summary>UTC timestamp this incident was opened.</summary>
    public UtcTimestamp OpenedAtUtc { get; private set; }

    /// <summary>UTC timestamp this incident was closed. Null while open.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    /// <summary>
    /// Elapsed minutes between opening and closing, computed at close time. Drives the "incident
    /// response time" KPI. Null while open.
    /// </summary>
    public int? ResponseTimeMinutes { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<ICyberPhysicalIncidentDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ICyberPhysicalIncidentDomainEvent> DomainEvents
        => new ReadOnlyCollection<ICyberPhysicalIncidentDomainEvent>(_domainEvents);

    // For rehydration only.
    private CyberPhysicalIncident() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Opens a new CyberPhysicalIncident and raises <see cref="CyberPhysicalIncidentOpenedEvent"/>.</summary>
    public static CyberPhysicalIncident Open(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid siteId,
        string title,
        string description,
        CyberPhysicalIncidentSeverity severity,
        UserId ownerId,
        IEnumerable<Guid>? relatedSabotageIndicatorIds = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new CyberPhysicalIncidentDomainException("Title is required to open a CyberPhysicalIncident.");

        var agg = new CyberPhysicalIncident
        {
            CyberPhysicalIncidentId = CyberPhysicalIncidentId.New(),
            TenantId                 = tenantId,
            SiteId                   = siteId,
            Title                    = title.Trim(),
            Description              = (description ?? string.Empty).Trim(),
            Severity                 = severity,
            Status                   = CyberPhysicalIncidentStatus.Open,
            OwnerId                  = ownerId,
            OpenedAtUtc              = createdAtUtc,
            CreatedAtUtc             = createdAtUtc,
            CreatedBy                = createdBy,
        };

        if (relatedSabotageIndicatorIds is not null)
            agg._relatedSabotageIndicatorIds.AddRange(relatedSabotageIndicatorIds);

        agg.AddDomainEvent(new CyberPhysicalIncidentOpenedEvent(
            EventId:                  CyberPhysicalIncidentEventId.New(),
            CyberPhysicalIncidentId:  agg.CyberPhysicalIncidentId,
            Severity:                 severity,
            OpenedAtUtc:              createdAtUtc,
            Actor:                    actor,
            CorrelationId:            correlationId,
            CausationId:              causationId));

        return agg;
    }

    // ---- Behavior ------------------------------------------------------------

    /// <summary>Advances the incident into active investigation.</summary>
    public void BeginInvestigation()
    {
        EnsureTransition(CyberPhysicalIncidentStatus.Investigating);
        Status = CyberPhysicalIncidentStatus.Investigating;
    }

    /// <summary>Marks the threat as contained.</summary>
    public void Contain()
    {
        EnsureTransition(CyberPhysicalIncidentStatus.Contained);
        Status = CyberPhysicalIncidentStatus.Contained;
    }

    /// <summary>Marks the incident as resolved, pending formal closure.</summary>
    public void Resolve()
    {
        EnsureTransition(CyberPhysicalIncidentStatus.Resolved);
        Status = CyberPhysicalIncidentStatus.Resolved;
    }

    /// <summary>Formally closes this incident and raises <see cref="CyberPhysicalIncidentClosedEvent"/>.</summary>
    public void Close(UtcTimestamp closedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureTransition(CyberPhysicalIncidentStatus.Closed);

        Status = CyberPhysicalIncidentStatus.Closed;
        ClosedAtUtc = closedAtUtc;
        ResponseTimeMinutes = (int)Math.Max(0, (closedAtUtc.Value - OpenedAtUtc.Value).TotalMinutes);

        AddDomainEvent(new CyberPhysicalIncidentClosedEvent(
            EventId:                  CyberPhysicalIncidentEventId.New(),
            CyberPhysicalIncidentId:  CyberPhysicalIncidentId,
            ResponseTimeMinutes:      ResponseTimeMinutes.Value,
            ClosedAtUtc:              closedAtUtc,
            Actor:                    actor,
            CorrelationId:            correlationId,
            CausationId:              causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ICyberPhysicalIncidentDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureTransition(CyberPhysicalIncidentStatus to)
    {
        if (!Rules.CyberPhysicalIncidentLifecycleRules.IsValidTransition(Status, to))
            throw new CyberPhysicalIncidentInvalidStatusTransitionException(Status, to);
    }
}
