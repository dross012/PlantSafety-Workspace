// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
//  Layer:          Domain
//  File:           AutonomousVehicleIncident.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single autonomous vehicle incident (collision/near-miss,
//                  geofence breach, software fault, or human-robot contact). Models recording,
//                  investigation, repeated-incident suspension-review triggering (feeding
//                  AutonomousVehicleAsset.Suspend), and closure. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Aggregates;

/// <summary>Aggregate root for a single autonomous vehicle incident.</summary>
public sealed partial class AutonomousVehicleIncident
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this AutonomousVehicleIncident aggregate.</summary>
    public AutonomousVehicleIncidentId AutonomousVehicleIncidentId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public AutonomousVehicleIncidentStatus Status { get; private set; }

    // ---- Incident detail --------------------------------------------------------

    /// <summary>Identity of the autonomous vehicle asset involved in the incident.</summary>
    public Guid AutonomousVehicleAssetId { get; private set; }

    /// <summary>Incident classification.</summary>
    public AutonomousVehicleIncidentType IncidentType { get; private set; }

    /// <summary>Severity classification.</summary>
    public IncidentSeverityLevel SeverityLevel { get; private set; }

    /// <summary>UTC timestamp the incident occurred.</summary>
    public UtcTimestamp OccurredAtUtc { get; private set; }

    /// <summary>Free-text summary of the incident.</summary>
    public string DescriptionSummary { get; private set; } = string.Empty;

    /// <summary>Identity of the user leading the investigation, if assigned.</summary>
    public UserId? InvestigationLeadUserId { get; private set; }

    /// <summary>Indicates whether a suspension review has been triggered for this incident.</summary>
    public bool SuspensionTriggered { get; private set; }

    /// <summary>UTC timestamp the incident was closed, if closed.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    // ---- Audit -------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -------------------------------------------------------

    private readonly List<IAutonomousVehicleIncidentDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAutonomousVehicleIncidentDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAutonomousVehicleIncidentDomainEvent>(_domainEvents);

    // For rehydration only.
    private AutonomousVehicleIncident() { }

    // ---- Factory: Create (Record) -----------------------------------------------

    /// <summary>Records a new autonomous vehicle incident and raises <see cref="AutonomousVehicleIncidentRecordedEvent"/>.</summary>
    public static AutonomousVehicleIncident Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid autonomousVehicleAssetId = default,
        AutonomousVehicleIncidentType incidentType = AutonomousVehicleIncidentType.CollisionNearMiss,
        IncidentSeverityLevel severityLevel = IncidentSeverityLevel.Medium,
        string descriptionSummary = "")
    {
        var agg = new AutonomousVehicleIncident
        {
            AutonomousVehicleIncidentId = AutonomousVehicleIncidentId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = AutonomousVehicleIncidentStatus.Reported,
            AutonomousVehicleAssetId = autonomousVehicleAssetId,
            IncidentType = incidentType,
            SeverityLevel = severityLevel,
            OccurredAtUtc = createdAtUtc,
            DescriptionSummary = (descriptionSummary ?? string.Empty).Trim(),
            SuspensionTriggered = false,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new AutonomousVehicleIncidentRecordedEvent(
            AutonomousVehicleIncidentEventId.New(), agg.AutonomousVehicleIncidentId, autonomousVehicleAssetId, incidentType, severityLevel, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: BeginInvestigation --------------------------------------------

    /// <summary>Begins the incident investigation.</summary>
    public void BeginInvestigation(UserId investigationLeadUserId, UtcTimestamp startedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(AutonomousVehicleIncidentStatus.Reported);

        InvestigationLeadUserId = investigationLeadUserId;
        Status = AutonomousVehicleIncidentStatus.UnderInvestigation;

        AddDomainEvent(new AutonomousVehicleIncidentInvestigationStartedEvent(
            AutonomousVehicleIncidentEventId.New(), AutonomousVehicleIncidentId, startedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: TriggerSuspensionReview ---------------------------------------

    /// <summary>Flags this incident as triggering an autonomous-operation suspension review (repeated-incidents risk hook).</summary>
    public void TriggerSuspensionReview(UtcTimestamp triggeredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(AutonomousVehicleIncidentStatus.Reported, AutonomousVehicleIncidentStatus.UnderInvestigation);

        SuspensionTriggered = true;
        Status = AutonomousVehicleIncidentStatus.CorrectiveActionInProgress;

        AddDomainEvent(new AutonomousVehicleIncidentSuspensionTriggeredEvent(
            AutonomousVehicleIncidentEventId.New(), AutonomousVehicleIncidentId, AutonomousVehicleAssetId, triggeredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Close -----------------------------------------------------------

    /// <summary>Closes the incident.</summary>
    public void Close(UtcTimestamp closedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == AutonomousVehicleIncidentStatus.Closed)
            throw new AutonomousVehicleIncidentInvalidStatusTransitionException(AutonomousVehicleIncidentId, Status.ToString(), AutonomousVehicleIncidentStatus.Closed.ToString(), correlationId, causationId);

        ClosedAtUtc = closedAtUtc;
        Status = AutonomousVehicleIncidentStatus.Closed;

        AddDomainEvent(new AutonomousVehicleIncidentClosedEvent(
            AutonomousVehicleIncidentEventId.New(), AutonomousVehicleIncidentId, closedAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IAutonomousVehicleIncidentDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params AutonomousVehicleIncidentStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for AutonomousVehicleIncident {AutonomousVehicleIncidentId}.");
    }
}
