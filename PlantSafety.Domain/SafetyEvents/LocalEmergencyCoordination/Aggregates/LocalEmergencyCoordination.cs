// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           LocalEmergencyCoordination.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the SafetyEvents.LocalEmergencyCoordination bounded context.
//                  Structured coordination workflows for external local emergency responders
//                  (fire, police, EMS, mutual-aid), complementing the internal-only
//                  EmergencyManagement/CrisisManagement sub-aggregates. Owns four child entity
//                  collections covering the module's four subdomains: Mutual Aid Agreements,
//                  Joint Response Planning, Live Incident Liaison (with communication logs), and
//                  Post-Incident Debrief with Agencies. Mirrors
//                  PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement's
//                  shape (Created/Active/Completed/Closed/Archived lifecycle, EnsureStatusIsOneOf
//                  guard pattern, CorrelationId/CausationId/PerformedByActor on every mutator).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates;

/// <summary>Aggregate root for the SafetyEvents.LocalEmergencyCoordination bounded context.</summary>
public sealed partial class LocalEmergencyCoordination
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this LocalEmergencyCoordination aggregate.</summary>
    public LocalEmergencyCoordinationId LocalEmergencyCoordinationId { get; private set; }

    /// <summary>Tenant that owns this coordination case.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site the coordination case relates to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public LocalEmergencyCoordinationStatus Status { get; private set; }

    // ---- LocalEmergencyCoordination-specific business properties -------------

    /// <summary>Identity of the SafetyEvents incident (EmergencyManagement/CrisisManagement) this coordination case relates to, if any.</summary>
    public Guid? IncidentId { get; private set; }

    /// <summary>Descriptive name for this coordination case.</summary>
    public string CaseName { get; private set; } = string.Empty;

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>UTC timestamp the case was closed.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    /// <summary>UTC timestamp the case was archived.</summary>
    public UtcTimestamp? ArchivedAtUtc { get; private set; }

    // ---- Child entity collections (Mutual Aid / Joint Response / Liaison / Debrief) ----

    private readonly List<Entities.MutualAidAgreement> _mutualAidAgreements = [];
    private readonly List<Entities.JointResponsePlan> _jointResponsePlans = [];
    private readonly List<Entities.ExternalResponderLiaison> _externalResponderLiaisons = [];
    private readonly List<Entities.JointDebriefRecord> _jointDebriefRecords = [];

    /// <summary>Mutual aid agreements established under this coordination case.</summary>
    public IReadOnlyCollection<Entities.MutualAidAgreement> MutualAidAgreements => new ReadOnlyCollection<Entities.MutualAidAgreement>(_mutualAidAgreements);

    /// <summary>Joint response plans activated under this coordination case.</summary>
    public IReadOnlyCollection<Entities.JointResponsePlan> JointResponsePlans => new ReadOnlyCollection<Entities.JointResponsePlan>(_jointResponsePlans);

    /// <summary>External responder liaison assignments under this coordination case.</summary>
    public IReadOnlyCollection<Entities.ExternalResponderLiaison> ExternalResponderLiaisons => new ReadOnlyCollection<Entities.ExternalResponderLiaison>(_externalResponderLiaisons);

    /// <summary>Joint post-incident debrief records under this coordination case.</summary>
    public IReadOnlyCollection<Entities.JointDebriefRecord> JointDebriefRecords => new ReadOnlyCollection<Entities.JointDebriefRecord>(_jointDebriefRecords);

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<Events.ILocalEmergencyCoordinationDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<Events.ILocalEmergencyCoordinationDomainEvent> DomainEvents
        => new ReadOnlyCollection<Events.ILocalEmergencyCoordinationDomainEvent>(_domainEvents);

    // For rehydration only.
    private LocalEmergencyCoordination() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new LocalEmergencyCoordination aggregate and raises <see cref="Events.LocalEmergencyCoordinationCreatedEvent"/>.</summary>
    public static LocalEmergencyCoordination Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid? incidentId = null,
        string caseName = "")
    {
        var agg = new LocalEmergencyCoordination
        {
            LocalEmergencyCoordinationId = LocalEmergencyCoordinationId.New(),
            TenantId                     = tenantId,
            SiteId                       = siteId,
            Status                       = LocalEmergencyCoordinationStatus.Created,
            IncidentId                   = incidentId,
            CaseName                     = (caseName ?? string.Empty).Trim(),
            CreatedAtUtc                 = createdAtUtc,
            CreatedBy                    = createdBy,
        };

        agg.AddDomainEvent(new Events.LocalEmergencyCoordinationCreatedEvent(
            EventId:       ValueObjects.Ids.LocalEmergencyCoordinationEventId.New(),
            LocalEmergencyCoordinationId: agg.LocalEmergencyCoordinationId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Subdomain 1: Mutual Aid Agreements ---------------------------------

    /// <summary>Establishes a new mutual aid agreement with an external local emergency responder agency (EPCRA/LEPC coordination support).</summary>
    public Entities.MutualAidAgreement EstablishMutualAidAgreement(
        string agencyName,
        string discipline,
        int documentVersion,
        UtcTimestamp effectiveFromUtc,
        UtcTimestamp renewalDueAtUtc,
        UserId establishedBy,
        UtcTimestamp establishedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, Rules.LocalEmergencyCoordinationLifecycleRules.CanCoordinate, LocalEmergencyCoordinationStatus.Active);

        if (Status == LocalEmergencyCoordinationStatus.Created)
            Status = LocalEmergencyCoordinationStatus.Active;

        var agreement = Entities.MutualAidAgreement.Create(
            LocalEmergencyCoordinationId, agencyName, discipline, documentVersion,
            effectiveFromUtc, renewalDueAtUtc, establishedAtUtc, establishedBy);

        _mutualAidAgreements.Add(agreement);

        AddDomainEvent(new Events.MutualAidAgreementEstablishedEvent(
            EventId: ValueObjects.Ids.LocalEmergencyCoordinationEventId.New(),
            LocalEmergencyCoordinationId: LocalEmergencyCoordinationId,
            MutualAidAgreementId: agreement.MutualAidAgreementId,
            AgencyName: agreement.AgencyName,
            EstablishedAtUtc: establishedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return agreement;
    }

    // ---- Subdomain 2: Joint Response Planning -------------------------------

    /// <summary>Activates a joint response plan coordinated with external local emergency responders for an incident.</summary>
    public Entities.JointResponsePlan ActivateJointResponsePlan(
        Guid? incidentId,
        string planName,
        UserId activatedBy,
        UtcTimestamp activatedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, Rules.LocalEmergencyCoordinationLifecycleRules.CanCoordinate, LocalEmergencyCoordinationStatus.Active);

        if (Status == LocalEmergencyCoordinationStatus.Created)
            Status = LocalEmergencyCoordinationStatus.Active;

        var plan = Entities.JointResponsePlan.Activate(
            LocalEmergencyCoordinationId, incidentId, planName, activatedAtUtc, activatedBy);

        _jointResponsePlans.Add(plan);

        AddDomainEvent(new Events.JointResponsePlanActivatedEvent(
            EventId: ValueObjects.Ids.LocalEmergencyCoordinationEventId.New(),
            LocalEmergencyCoordinationId: LocalEmergencyCoordinationId,
            JointResponsePlanId: plan.JointResponsePlanId,
            IncidentId: incidentId,
            ActivatedAtUtc: activatedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return plan;
    }

    /// <summary>Deactivates a previously activated joint response plan.</summary>
    public void DeactivateJointResponsePlan(
        JointResponsePlanId jointResponsePlanId,
        UserId deactivatedBy,
        UtcTimestamp deactivatedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, Rules.LocalEmergencyCoordinationLifecycleRules.CanCoordinate, LocalEmergencyCoordinationStatus.Active);

        var plan = _jointResponsePlans.SingleOrDefault(p => p.JointResponsePlanId.Equals(jointResponsePlanId))
            ?? throw new Exceptions.JointResponsePlanNotFoundException(jointResponsePlanId, correlationId, causationId);

        plan.Deactivate(deactivatedAtUtc, deactivatedBy);

        AddDomainEvent(new Events.JointResponsePlanDeactivatedEvent(
            EventId: ValueObjects.Ids.LocalEmergencyCoordinationEventId.New(),
            LocalEmergencyCoordinationId: LocalEmergencyCoordinationId,
            JointResponsePlanId: plan.JointResponsePlanId,
            DeactivatedAtUtc: deactivatedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Subdomain 3: Live Incident Liaison ---------------------------------

    /// <summary>Assigns an internal user as the live-incident liaison to a specific external responder agency.</summary>
    public Entities.ExternalResponderLiaison AssignExternalResponderLiaison(
        Guid? incidentId,
        string agencyName,
        UserId liaisonUserId,
        UserId assignedBy,
        UtcTimestamp assignedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, Rules.LocalEmergencyCoordinationLifecycleRules.CanCoordinate, LocalEmergencyCoordinationStatus.Active);

        if (Status == LocalEmergencyCoordinationStatus.Created)
            Status = LocalEmergencyCoordinationStatus.Active;

        var liaison = Entities.ExternalResponderLiaison.Assign(
            LocalEmergencyCoordinationId, incidentId, agencyName, liaisonUserId, assignedAtUtc, assignedBy);

        _externalResponderLiaisons.Add(liaison);

        AddDomainEvent(new Events.ExternalResponderLiaisonAssignedEvent(
            EventId: ValueObjects.Ids.LocalEmergencyCoordinationEventId.New(),
            LocalEmergencyCoordinationId: LocalEmergencyCoordinationId,
            ExternalResponderLiaisonId: liaison.ExternalResponderLiaisonId,
            AgencyName: liaison.AgencyName,
            AssignedAtUtc: assignedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return liaison;
    }

    /// <summary>
    /// Logs a communication exchange with an external responder agency during a live incident.
    /// Retained with the incident record for the full incident retention period.
    /// </summary>
    public Entities.LiaisonLogEntry LogLiaisonCommunication(
        ExternalResponderLiaisonId externalResponderLiaisonId,
        string message,
        UserId loggedBy,
        UtcTimestamp loggedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, Rules.LocalEmergencyCoordinationLifecycleRules.CanCoordinate, LocalEmergencyCoordinationStatus.Active);

        var liaison = _externalResponderLiaisons.SingleOrDefault(l => l.ExternalResponderLiaisonId.Equals(externalResponderLiaisonId))
            ?? throw new Exceptions.ExternalResponderLiaisonNotFoundException(externalResponderLiaisonId, correlationId, causationId);

        var entry = liaison.LogCommunication(message, loggedAtUtc, loggedBy);

        AddDomainEvent(new Events.LiaisonCommunicationLoggedEvent(
            EventId: ValueObjects.Ids.LocalEmergencyCoordinationEventId.New(),
            LocalEmergencyCoordinationId: LocalEmergencyCoordinationId,
            ExternalResponderLiaisonId: liaison.ExternalResponderLiaisonId,
            LiaisonLogEntryId: entry.LiaisonLogEntryId,
            LoggedAtUtc: loggedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return entry;
    }

    // ---- Subdomain 4: Post-Incident Debrief with Agencies -------------------

    /// <summary>Records a post-incident debrief conducted jointly with external local emergency responder agencies.</summary>
    public Entities.JointDebriefRecord RecordJointDebrief(
        Guid? incidentId,
        string agenciesInvolved,
        string summary,
        UserId recordedBy,
        UtcTimestamp recordedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, Rules.LocalEmergencyCoordinationLifecycleRules.CanCoordinate, LocalEmergencyCoordinationStatus.Active);

        var debrief = Entities.JointDebriefRecord.Create(
            LocalEmergencyCoordinationId, incidentId, agenciesInvolved, summary, recordedAtUtc, recordedBy);

        _jointDebriefRecords.Add(debrief);

        AddDomainEvent(new Events.JointDebriefRecordedEvent(
            EventId: ValueObjects.Ids.LocalEmergencyCoordinationEventId.New(),
            LocalEmergencyCoordinationId: LocalEmergencyCoordinationId,
            JointDebriefRecordId: debrief.JointDebriefRecordId,
            IncidentId: incidentId,
            RecordedAtUtc: recordedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return debrief;
    }

    // ---- Lifecycle: Complete / Close / Archive ------------------------------

    /// <summary>Completes the coordination case (Active -&gt; Completed).</summary>
    public void Complete(
        UserId completedBy,
        UtcTimestamp completedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, Rules.LocalEmergencyCoordinationLifecycleRules.CanComplete, LocalEmergencyCoordinationStatus.Completed);

        Status = LocalEmergencyCoordinationStatus.Completed;
    }

    /// <summary>Closes the coordination case (Created/Active/Completed -&gt; Closed).</summary>
    public void Close(
        UserId closedBy,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, Rules.LocalEmergencyCoordinationLifecycleRules.CanClose, LocalEmergencyCoordinationStatus.Closed);

        Status = LocalEmergencyCoordinationStatus.Closed;
        ClosedAtUtc = closedAtUtc;

        AddDomainEvent(new Events.LocalEmergencyCoordinationClosedEvent(
            EventId: ValueObjects.Ids.LocalEmergencyCoordinationEventId.New(),
            LocalEmergencyCoordinationId: LocalEmergencyCoordinationId,
            ClosedBy: closedBy,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Archives a closed coordination case for EPCRA/LEPC compliance retention (Closed -&gt; Archived).</summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, Rules.LocalEmergencyCoordinationLifecycleRules.CanArchive, LocalEmergencyCoordinationStatus.Archived);

        Status = LocalEmergencyCoordinationStatus.Archived;
        ArchivedAtUtc = archivedAtUtc;

        AddDomainEvent(new Events.LocalEmergencyCoordinationArchivedEvent(
            EventId: ValueObjects.Ids.LocalEmergencyCoordinationEventId.New(),
            LocalEmergencyCoordinationId: LocalEmergencyCoordinationId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(Events.ILocalEmergencyCoordinationDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>For read-model/ORM rehydration of the child entity collections.</summary>
    internal void RehydrateChildren(
        IEnumerable<Entities.MutualAidAgreement> agreements,
        IEnumerable<Entities.JointResponsePlan> plans,
        IEnumerable<Entities.ExternalResponderLiaison> liaisons,
        IEnumerable<Entities.JointDebriefRecord> debriefs)
    {
        _mutualAidAgreements.Clear();
        _mutualAidAgreements.AddRange(agreements);
        _jointResponsePlans.Clear();
        _jointResponsePlans.AddRange(plans);
        _externalResponderLiaisons.Clear();
        _externalResponderLiaisons.AddRange(liaisons);
        _jointDebriefRecords.Clear();
        _jointDebriefRecords.AddRange(debriefs);
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<LocalEmergencyCoordinationStatus, bool> guard, LocalEmergencyCoordinationStatus expected)
    {
        if (guard(Status)) return;

        throw new Exceptions.LocalEmergencyCoordinationInvalidStatusTransitionException(
            LocalEmergencyCoordinationId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
