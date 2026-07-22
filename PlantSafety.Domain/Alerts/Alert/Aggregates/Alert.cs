// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           Alert.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Alerts bounded context. Represents a single triggered
//                  manufacturing safety alert (life-safety emergency, man-down, operational
//                  hazard, environmental, facility/traffic, PPE/behavioral, PTW, maintenance,
//                  security, quality, or compliance) through its full lifecycle: triggered,
//                  acknowledged, dispatched, (optionally) escalated, resolved, and closed with
//                  close-out verification. Grounded in
//                  PlantSafety_Program_Structure_extract.txt "Comprehensive Manufacturing Safety
//                  Alerts" (catalog, severity levels, logging & reporting) and
//                  "11. Alerts & Emergency Response Dashboard".
//                  Additive-only evolution.
// ============================================================================
// NOTE: This module's type names intentionally collide with the pre-existing
// Operational.Alerts "canonical reference module" (both are project-wide, but Operational.Alerts
// is globally `using`-imported in GlobalUsing.cs). To avoid CS0104 ambiguity without modifying
// any pre-existing file, every cross-namespace reference below uses an explicit using-alias
// directive, which always takes precedence over using-namespace directives during name
// resolution.
using AlertResponseActionEntry = PlantSafety.Domain.Alerts.Alert.Entities.AlertResponseActionEntry;
using AlertDeliveryRecord = PlantSafety.Domain.Alerts.Alert.Entities.AlertDeliveryRecord;
using AlertCategory = PlantSafety.Domain.Alerts.Alert.Enums.AlertCategory;
using AlertSeverity = PlantSafety.Domain.Alerts.Alert.Enums.AlertSeverity;
using AlertStatus = PlantSafety.Domain.Alerts.Alert.Enums.AlertStatus;
using AlertSourceType = PlantSafety.Domain.Alerts.Alert.Enums.AlertSourceType;
using AlertDeliveryChannel = PlantSafety.Domain.Alerts.Alert.Enums.AlertDeliveryChannel;
using IAlertDomainEvent = PlantSafety.Domain.Alerts.Alert.Events.IAlertDomainEvent;
using AlertTriggeredEvent = PlantSafety.Domain.Alerts.Alert.Events.AlertTriggeredEvent;
using AlertAcknowledgedEvent = PlantSafety.Domain.Alerts.Alert.Events.AlertAcknowledgedEvent;
using AlertDispatchedEvent = PlantSafety.Domain.Alerts.Alert.Events.AlertDispatchedEvent;
using AlertEscalatedEvent = PlantSafety.Domain.Alerts.Alert.Events.AlertEscalatedEvent;
using AlertResolvedEvent = PlantSafety.Domain.Alerts.Alert.Events.AlertResolvedEvent;
using AlertClosedEvent = PlantSafety.Domain.Alerts.Alert.Events.AlertClosedEvent;
using AlertDeliveryRecordedEvent = PlantSafety.Domain.Alerts.Alert.Events.AlertDeliveryRecordedEvent;
using AlertInvalidStatusTransitionException = PlantSafety.Domain.Alerts.Alert.Exceptions.AlertInvalidStatusTransitionException;
using AlertLifecycleRules = PlantSafety.Domain.Alerts.Alert.Rules.AlertLifecycleRules;
using AlertLocation = PlantSafety.Domain.Alerts.Alert.ValueObjects.AlertLocation;
using AlertId = PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids.AlertId;
using AlertEventId = PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids.AlertEventId;

namespace PlantSafety.Domain.Alerts.Alert.Aggregates;

/// <summary>Aggregate root for the Alerts bounded context.</summary>
public sealed partial class Alert
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this Alert aggregate.</summary>
    public AlertId AlertId { get; private set; }

    /// <summary>Tenant that owns this alert record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this alert is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public AlertStatus Status { get; private set; }

    // ---- Alert-specific business properties -----------------------------------

    /// <summary>Catalog category classifying this alert (drives default severity and response instructions).</summary>
    public AlertCategory Category { get; private set; } = AlertCategory.Default;

    /// <summary>Current severity classification.</summary>
    public AlertSeverity Severity { get; private set; } = AlertSeverity.Default;

    /// <summary>Origin that triggered this alert (sensor, computer vision, human, etc.).</summary>
    public AlertSourceType SourceType { get; private set; }

    /// <summary>Short human-readable alert title/summary.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Physical/logical location this alert was raised at.</summary>
    public AlertLocation Location { get; private set; } = AlertLocation.Unknown;

    /// <summary>UTC instant the alert was triggered.</summary>
    public UtcTimestamp TriggeredAtUtc { get; private set; }

    /// <summary>UTC instant the alert was acknowledged, if acknowledged.</summary>
    public UtcTimestamp? AcknowledgedAtUtc { get; private set; }

    /// <summary>UTC instant the alert was resolved, if resolved.</summary>
    public UtcTimestamp? ResolvedAtUtc { get; private set; }

    /// <summary>UTC instant the alert was closed, if closed.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    /// <summary>Free-text close-out verification recorded at closure.</summary>
    public string? CloseOutVerification { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship). Empty for system-triggered alerts.</summary>
    public UserId? CreatedBy { get; private set; }

    // ---- Child collections ------------------------------------------------

    private readonly List<AlertResponseActionEntry> _responseActions = [];

    /// <summary>Response actions recorded against this alert.</summary>
    public IReadOnlyList<AlertResponseActionEntry> ResponseActions => _responseActions.AsReadOnly();

    private readonly List<AlertDeliveryRecord> _deliveryRecords = [];

    /// <summary>Multi-channel delivery attempts recorded against this alert.</summary>
    public IReadOnlyList<AlertDeliveryRecord> DeliveryRecords => _deliveryRecords.AsReadOnly();

    // ---- Domain Events ------------------------------------------------------

    private readonly List<IAlertDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAlertDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAlertDomainEvent>(_domainEvents);

    // For rehydration only.
    private Alert() { }

    // ---- Factory --------------------------------------------------------------

    /// <summary>Triggers a new Alert aggregate and raises <see cref="AlertTriggeredEvent"/>.</summary>
    /// <param name="triggeredAtUtc">Trigger timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity (user, sensor, system, etc.).</param>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="siteId">Optional owning site.</param>
    /// <param name="category">Catalog category. Determines default severity when <paramref name="severity"/> is null.</param>
    /// <param name="sourceType">Origin that triggered the alert.</param>
    /// <param name="title">Short human-readable summary.</param>
    /// <param name="location">Physical/logical location. Defaults to <see cref="AlertLocation.Unknown"/>.</param>
    /// <param name="severity">Optional explicit severity override; defaults to the category's default severity.</param>
    /// <param name="createdBy">Optional CRUD authorship identity. Null for system/sensor-triggered alerts.</param>
    public static Alert Trigger(
        UtcTimestamp triggeredAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        AlertCategory category,
        AlertSourceType sourceType,
        string title,
        AlertLocation? location = null,
        AlertSeverity? severity = null,
        UserId? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Alert.Title must not be null or whitespace.", nameof(title));

        var effectiveCategory = category ?? AlertCategory.Default;

        var agg = new Alert
        {
            AlertId        = AlertId.New(),
            TenantId       = tenantId,
            SiteId         = siteId,
            Status         = AlertStatus.Triggered,
            Category       = effectiveCategory,
            Severity       = severity ?? effectiveCategory.DefaultSeverity,
            SourceType     = sourceType,
            Title          = title.Trim(),
            Location       = location ?? AlertLocation.Unknown,
            TriggeredAtUtc = triggeredAtUtc,
            CreatedAtUtc   = triggeredAtUtc,
            CreatedBy      = createdBy,
        };

        agg.AddDomainEvent(new AlertTriggeredEvent(
            EventId:        AlertEventId.New(),
            AlertId:        agg.AlertId,
            Category:       agg.Category,
            Severity:       agg.Severity,
            SourceType:     agg.SourceType,
            TriggeredAtUtc: triggeredAtUtc,
            Actor:          actor,
            CorrelationId:  correlationId,
            CausationId:    causationId));

        return agg;
    }

    // ---- Behavior -----------------------------------------------------------

    /// <summary>Acknowledges the alert. Valid only from <see cref="AlertStatus.Triggered"/> or <see cref="AlertStatus.Escalated"/>.</summary>
    public void Acknowledge(UtcTimestamp acknowledgedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!AlertLifecycleRules.CanAcknowledge(Status))
            throw new AlertInvalidStatusTransitionException(AlertId, Status.ToString(), AlertStatus.Acknowledged.ToString(), correlationId, causationId);

        Status = AlertStatus.Acknowledged;
        AcknowledgedAtUtc = acknowledgedAtUtc;

        AddDomainEvent(new AlertAcknowledgedEvent(AlertEventId.New(), AlertId, acknowledgedAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Dispatches a response for the alert. Valid only from <see cref="AlertStatus.Acknowledged"/>.</summary>
    public void Dispatch(string dispatchedTo, UtcTimestamp dispatchedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!AlertLifecycleRules.CanDispatch(Status))
            throw new AlertInvalidStatusTransitionException(AlertId, Status.ToString(), AlertStatus.Dispatched.ToString(), correlationId, causationId);

        if (string.IsNullOrWhiteSpace(dispatchedTo))
            throw new ArgumentException("DispatchedTo must not be null or whitespace.", nameof(dispatchedTo));

        Status = AlertStatus.Dispatched;

        AddDomainEvent(new AlertDispatchedEvent(AlertEventId.New(), AlertId, dispatchedTo.Trim(), dispatchedAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Escalates the alert, manually or automatically. Valid from <see cref="AlertStatus.Triggered"/> or <see cref="AlertStatus.Acknowledged"/>.</summary>
    public void Escalate(string escalationReason, bool wasAutoEscalated, UtcTimestamp escalatedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!AlertLifecycleRules.CanEscalate(Status))
            throw new AlertInvalidStatusTransitionException(AlertId, Status.ToString(), AlertStatus.Escalated.ToString(), correlationId, causationId);

        if (string.IsNullOrWhiteSpace(escalationReason))
            throw new ArgumentException("EscalationReason must not be null or whitespace.", nameof(escalationReason));

        Status = AlertStatus.Escalated;

        AddDomainEvent(new AlertEscalatedEvent(AlertEventId.New(), AlertId, escalationReason.Trim(), wasAutoEscalated, escalatedAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Resolves the underlying hazard/condition. Valid from <see cref="AlertStatus.Acknowledged"/> or <see cref="AlertStatus.Escalated"/>.</summary>
    public void Resolve(string responseActionsTaken, UtcTimestamp resolvedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!AlertLifecycleRules.CanResolve(Status))
            throw new AlertInvalidStatusTransitionException(AlertId, Status.ToString(), AlertStatus.Resolved.ToString(), correlationId, causationId);

        Status = AlertStatus.Resolved;
        ResolvedAtUtc = resolvedAtUtc;

        AddDomainEvent(new AlertResolvedEvent(AlertEventId.New(), AlertId, responseActionsTaken ?? string.Empty, resolvedAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Closes the alert with close-out verification. Valid only from <see cref="AlertStatus.Resolved"/>.</summary>
    public void Close(string closeOutVerification, UtcTimestamp closedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!AlertLifecycleRules.CanClose(Status))
            throw new AlertInvalidStatusTransitionException(AlertId, Status.ToString(), AlertStatus.Closed.ToString(), correlationId, causationId);

        if (string.IsNullOrWhiteSpace(closeOutVerification))
            throw new ArgumentException("CloseOutVerification must not be null or whitespace.", nameof(closeOutVerification));

        Status = AlertStatus.Closed;
        ClosedAtUtc = closedAtUtc;
        CloseOutVerification = closeOutVerification.Trim();

        AddDomainEvent(new AlertClosedEvent(AlertEventId.New(), AlertId, CloseOutVerification, closedAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Records a single response action taken against this alert.</summary>
    public void RecordResponseAction(Guid responseActionId, string actionDescription, UtcTimestamp takenAtUtc, PerformedByActor actor, string? notes = null)
        => _responseActions.Add(new AlertResponseActionEntry(responseActionId, AlertId, actionDescription, takenAtUtc, actor, notes));

    /// <summary>Records a single multi-channel delivery attempt for this alert.</summary>
    public void RecordDelivery(Guid deliveryRecordId, AlertDeliveryChannel channel, bool succeeded, UtcTimestamp attemptedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId, string? failureReason = null)
    {
        _deliveryRecords.Add(new AlertDeliveryRecord(deliveryRecordId, AlertId, channel, succeeded, attemptedAtUtc, failureReason));

        AddDomainEvent(new AlertDeliveryRecordedEvent(AlertEventId.New(), AlertId, channel, succeeded, attemptedAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IAlertDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
