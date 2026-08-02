// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Domain
//  File:           EmergencyManagement.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  emergency-response record shape grounded in
//                  project_plantsafety_domains.md Module 9 (Emergency: "Emergency
//                  declaration, response coordination, stabilization, and closure",
//                  flagged as the highest business-continuity-priority Critical
//                  Module). Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the EmergencyManagement bounded context. Tracks
//                  a single emergency event: its type, severity, location, casualty
//                  count, notification state, and stabilization/resolution timeline.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates;

/// <summary>Aggregate root for the EmergencyManagement bounded context.</summary>
public sealed partial class EmergencyManagement
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this EmergencyManagement aggregate.</summary>
    public EmergencyManagementId EmergencyManagementId { get; private set; }

    /// <summary>Tenant that owns this emergency record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site the emergency occurred at. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public EmergencyManagementStatus Status { get; private set; }

    // ---- Emergency-specific business properties ---------------------------------

    /// <summary>Emergency classification (e.g., Fire, ChemicalRelease, MedicalEmergency, Evacuation, SevereWeather).</summary>
    public string EmergencyType { get; private set; } = string.Empty;

    /// <summary>Emergency severity: Minor, Moderate, Major, Catastrophic.</summary>
    public string Severity { get; private set; } = "Minor";

    /// <summary>Physical location the emergency was declared at.</summary>
    public string Location { get; private set; } = string.Empty;

    /// <summary>Count of casualties/injuries associated with this emergency.</summary>
    public int CasualtyCount { get; private set; }

    /// <summary>Indicates whether the emergency response team has been notified.</summary>
    public bool ResponseTeamNotified { get; private set; }

    /// <summary>UTC timestamp the emergency was declared.</summary>
    public UtcTimestamp DeclaredAtUtc { get; private set; }

    /// <summary>UTC timestamp the emergency was stabilized.</summary>
    public UtcTimestamp? StabilizedAtUtc { get; private set; }

    /// <summary>UTC timestamp the emergency was resolved/closed.</summary>
    public UtcTimestamp? ResolvedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IEmergencyManagementDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IEmergencyManagementDomainEvent> DomainEvents
        => new ReadOnlyCollection<IEmergencyManagementDomainEvent>(_domainEvents);

    // For rehydration only.
    private EmergencyManagement() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new EmergencyManagement aggregate and raises <see cref="EmergencyManagementCreatedEvent"/>.</summary>
    public static EmergencyManagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string emergencyType = "",
        string severity = "Minor",
        string location = "",
        int casualtyCount = 0,
        bool responseTeamNotified = false)
    {
        var agg = new EmergencyManagement
        {
            EmergencyManagementId  = EmergencyManagementId.New(),
            TenantId               = tenantId,
            SiteId                 = siteId,
            Status                 = EmergencyManagementStatus.Created,
            EmergencyType          = (emergencyType ?? string.Empty).Trim(),
            Severity               = (severity ?? "Minor").Trim(),
            Location               = (location ?? string.Empty).Trim(),
            CasualtyCount          = casualtyCount < 0 ? 0 : casualtyCount,
            ResponseTeamNotified   = responseTeamNotified,
            DeclaredAtUtc          = createdAtUtc,
            CreatedAtUtc           = createdAtUtc,
            CreatedBy              = createdBy,
        };

        agg.AddDomainEvent(new EmergencyManagementCreatedEvent(
            EventId:       new EmergencyManagementEventId(Uuid7.New()),
            EmergencyManagementId:      agg.EmergencyManagementId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: RespondTo / Stabilize ------------------------------------

    /// <summary>
    /// Records that the response team has begun responding to the declared emergency
    /// (Created -&gt; Active). Valid only from <see cref="EmergencyManagementStatus.Created"/>.
    /// </summary>
    public void RespondTo(
        UserId respondedBy,
        UtcTimestamp respondedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EmergencyManagementLifecycleRules.CanActivate, EmergencyManagementStatus.Active);

        ResponseTeamNotified = true;
        Status = EmergencyManagementStatus.Active;

        AddDomainEvent(new EmergencyManagementRespondedToEvent(
            EventId: new EmergencyManagementEventId(Uuid7.New()),
            EmergencyManagementId: EmergencyManagementId,
            RespondedBy: respondedBy,
            RespondedAtUtc: respondedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Records that the emergency has been stabilized during active response. Does not change
    /// lifecycle status. Valid only while <see cref="EmergencyManagementStatus.Active"/>.
    /// </summary>
    public void Stabilize(
        UserId stabilizedBy,
        UtcTimestamp stabilizedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EmergencyManagementLifecycleRules.CanStabilize, EmergencyManagementStatus.Active);

        StabilizedAtUtc = stabilizedAtUtc;

        AddDomainEvent(new EmergencyManagementStabilizedEvent(
            EventId: new EmergencyManagementEventId(Uuid7.New()),
            EmergencyManagementId: EmergencyManagementId,
            StabilizedBy: stabilizedBy,
            StabilizedAtUtc: stabilizedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Resolve / Close / Archive --------------------------------

    /// <summary>
    /// Resolves the emergency (Active -&gt; Completed). Valid only from
    /// <see cref="EmergencyManagementStatus.Active"/>.
    /// </summary>
    public void Resolve(
        UserId resolvedBy,
        UtcTimestamp resolvedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EmergencyManagementLifecycleRules.CanComplete, EmergencyManagementStatus.Completed);

        ResolvedAtUtc = resolvedAtUtc;
        Status = EmergencyManagementStatus.Completed;

        AddDomainEvent(new EmergencyManagementResolvedEvent(
            EventId: new EmergencyManagementEventId(Uuid7.New()),
            EmergencyManagementId: EmergencyManagementId,
            ResolvedBy: resolvedBy,
            ResolvedAtUtc: resolvedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Closes a resolved emergency record (Completed -&gt; Closed). Valid only from
    /// <see cref="EmergencyManagementStatus.Completed"/>.
    /// </summary>
    public void Close(
        UserId closedBy,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EmergencyManagementLifecycleRules.CanClose, EmergencyManagementStatus.Closed);

        Status = EmergencyManagementStatus.Closed;

        AddDomainEvent(new EmergencyManagementClosedEvent(
            EventId: new EmergencyManagementEventId(Uuid7.New()),
            EmergencyManagementId: EmergencyManagementId,
            ClosedBy: closedBy,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Archives a closed emergency record for compliance retention (Closed -&gt; Archived). Valid
    /// only from <see cref="EmergencyManagementStatus.Closed"/>.
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EmergencyManagementLifecycleRules.CanArchive, EmergencyManagementStatus.Archived);

        Status = EmergencyManagementStatus.Archived;

        AddDomainEvent(new EmergencyManagementArchivedEvent(
            EventId: new EmergencyManagementEventId(Uuid7.New()),
            EmergencyManagementId: EmergencyManagementId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IEmergencyManagementDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params EmergencyManagementStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for EmergencyManagement {EmergencyManagementId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<EmergencyManagementStatus, bool> guard, EmergencyManagementStatus expected)
    {
        if (guard(Status)) return;

        throw new EmergencyManagementInvalidStatusTransitionException(
            EmergencyManagementId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
