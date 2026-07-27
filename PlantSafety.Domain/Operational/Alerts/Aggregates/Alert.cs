// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           Alert.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Canonical Alert aggregate root implementing the full lifecycle:
//          Created → Acknowledged → InProgress → Escalated → Resolved → Closed → Archived
//      Complies with:
//          - Canonical Aggregate Rule (2.5)
//          - Deterministic Time Rule (1.1)
//          - Deterministic Identifier Rule (1.2)
//          - Actor & User Identity Semantics Rule (1.8)
//          - Domain Events Architectural Rule (2.13)
//          - Event Envelope Rule (12.5)
//      Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;
using PlantSafety.Domain.Operational.Alerts.Events;
using PlantSafety.Domain.Operational.Alerts.Snapshots;

namespace PlantSafety.Domain.Operational.Alerts.Aggregates;

public sealed partial class Alert : IHasDomainEvents
{
    /// <summary>
    /// Explicit <see cref="IHasDomainEvents"/> projection of <see cref="DomainEvents"/>.
    /// IReadOnlyCollection&lt;IAlertDomainEvent&gt; cannot be used directly to satisfy
    /// IReadOnlyCollection&lt;IDomainEvent&gt; via interface implementation (no member-level
    /// covariance for property implementations), so it is materialized explicitly here.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> IHasDomainEvents.DomainEvents
        => DomainEvents.Cast<IDomainEvent>().ToList();

    // ------------------------------------------------------------------------
    // Core Identity & Lifecycle
    // ------------------------------------------------------------------------

    public AlertId AlertId { get; private set; }
    public AlertType AlertType { get; private set; }
    public AlertSeverity Severity { get; private set; }
    public AlertStatus Status { get; private set; }

    public string Title { get; private set; }
    public string Description { get; private set; }
    public string SourceModule { get; private set; }

    public UtcTimestamp CreatedAtUtc { get; private set; }
    public UserId CreatedBy { get; private set; }

    public UtcTimestamp? AcknowledgedAtUtc { get; private set; }
    public UtcTimestamp? InProgressAtUtc { get; private set; }
    public UtcTimestamp? EscalatedAtUtc { get; private set; }
    public UtcTimestamp? ResolvedAtUtc { get; private set; }
    public UtcTimestamp? ClosedAtUtc { get; private set; }
    public UtcTimestamp? ArchivedAtUtc { get; private set; }

    public PerformedByActor? AcknowledgedBy { get; private set; }
    public PerformedByActor? InProgressBy { get; private set; }
    public PerformedByActor? EscalatedBy { get; private set; }
    public PerformedByActor? ResolvedBy { get; private set; }
    public PerformedByActor? ClosedBy { get; private set; }
    public PerformedByActor? ArchivedBy { get; private set; }

    public EscalationReason? EscalationReason { get; private set; }
    public ResolutionSummary? ResolutionSummary { get; private set; }
    public AlertNotes? Notes { get; private set; }

    public SlaExpiration? SlaExpiration { get; private set; }

    // ------------------------------------------------------------------------
    // Child Collections
    // ------------------------------------------------------------------------

    private readonly List<AlertHistoryEntry> _history = [];
    public IReadOnlyCollection<AlertHistoryEntry> History => new ReadOnlyCollection<AlertHistoryEntry>(_history);

    private readonly List<AlertAssignment> _assignments = [];
    public IReadOnlyCollection<AlertAssignment> Assignments => new ReadOnlyCollection<AlertAssignment>(_assignments);

    private readonly List<AlertMetadataEntry> _metadata = [];
    public IReadOnlyCollection<AlertMetadataEntry> Metadata => new ReadOnlyCollection<AlertMetadataEntry>(_metadata);

    // ------------------------------------------------------------------------
    // Domain Events
    // ------------------------------------------------------------------------

    private readonly List<IAlertDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IAlertDomainEvent> DomainEvents => new ReadOnlyCollection<IAlertDomainEvent>(_domainEvents);

    internal Alert() { } // For rehydration only (accessible within PlantSafety.Domain to rebuild/rehydration infrastructure)

    // ------------------------------------------------------------------------
    // Factory
    // ------------------------------------------------------------------------

    public static Alert Create(
        AlertType alertType,
        AlertSeverity severity,
        string title,
        string description,
        string sourceModule,
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        if (string.IsNullOrWhiteSpace(sourceModule))
            throw new ArgumentException("SourceModule is required.", nameof(sourceModule));

        var alert = new Alert
        {
            AlertId = AlertId.New(),
            AlertType = alertType,
            Severity = severity,
            Status = AlertStatus.Created,
            Title = title.Trim(),
            Description = description.Trim(),
            SourceModule = sourceModule.Trim(),
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy
        };

        alert.AddHistory(AlertStatus.Created, AlertStatus.Created, createdAtUtc, actor, null);

        var evt = new AlertCreatedEvent(
            EventId: new AlertEventId(Uuid7.New()),
            AlertId: alert.AlertId,
            AlertType: alert.AlertType,
            Severity: alert.Severity,
            Title: alert.Title,
            Description: alert.Description,
            SourceModule: alert.SourceModule,
            CreatedAtUtc: createdAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId);

        alert.AddDomainEvent(evt);

        return alert;
    }

    /// <summary>
    /// Convenience overload matching the lighter Create(createdBy, createdAtUtc,
    /// correlationId, causationId, actor) convention used by other aggregates
    /// (e.g. Culture, Dashboard). Business fields default to a generic
    /// worker-safety alert; callers needing full control should use the
    /// primary <see cref="Create(AlertType, AlertSeverity, string, string, string, UserId, UtcTimestamp, CorrelationId, CausationId, PerformedByActor)"/>
    /// overload instead.
    /// </summary>
    public static Alert Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        AlertType? defaultAlertType = null,
        AlertSeverity? defaultSeverity = null,
        string defaultTitle = "Untitled Alert",
        string defaultDescription = "No description provided.",
        string defaultSourceModule = "Unspecified")
        => Create(
            alertType: defaultAlertType ?? AlertType.WorkerSafety,
            severity: defaultSeverity ?? AlertSeverity.Default,
            title: defaultTitle,
            description: defaultDescription,
            sourceModule: defaultSourceModule,
            createdBy: createdBy,
            createdAtUtc: createdAtUtc,
            correlationId: correlationId,
            causationId: causationId,
            actor: actor);

    // ------------------------------------------------------------------------
    // SLA
    // ------------------------------------------------------------------------

    /// <summary>
    /// Sets the SLA expiration for this alert. Invoked by the factory/SLA
    /// policy immediately after creation (or by re-evaluation policies later
    /// in the lifecycle). Does not itself emit a domain event; SLA changes
    /// are considered derived/computed state rather than a lifecycle transition.
    /// </summary>
    public void SetSlaExpiration(SlaExpiration slaExpiration)
    {
        SlaExpiration = slaExpiration;
    }

    // ------------------------------------------------------------------------
    // Metadata
    // ------------------------------------------------------------------------

    /// <summary>
    /// Adds a metadata entry to this alert. Key uniqueness and normalization
    /// are enforced by the caller (see <c>AlertMetadataService</c>).
    /// </summary>
    public void AddMetadata(AlertMetadataKey key, AlertMetadataValue value)
    {
        var entry = new AlertMetadataEntry(
            alertMetadataId: AlertMetadataId.New(),
            alertId: AlertId,
            key: key,
            value: value);

        _metadata.Add(entry);
    }

    // ------------------------------------------------------------------------
    // Lifecycle Methods
    // ------------------------------------------------------------------------

    public void Acknowledge(
        UtcTimestamp acknowledgedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        AlertNotes? notes = null)
    {
        EnsureStatusIsOneOf(AlertStatus.Created);

        var previous = Status;
        Status = AlertStatus.Acknowledged;
        AcknowledgedAtUtc = acknowledgedAtUtc;
        AcknowledgedBy = actor;
        Notes = notes ?? Notes;

        AddHistory(previous, Status, acknowledgedAtUtc, actor, notes?.Value);

        var evt = new AlertAcknowledgedEvent(
            EventId: new AlertEventId(Uuid7.New()),
            AlertId: AlertId,
            AcknowledgedAtUtc: acknowledgedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId);

        AddDomainEvent(evt);
    }

    public void StartProgress(
        UtcTimestamp inProgressAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(AlertStatus.Created, AlertStatus.Acknowledged);

        var previous = Status;
        Status = AlertStatus.InProgress;
        InProgressAtUtc = inProgressAtUtc;
        InProgressBy = actor;

        AddHistory(previous, Status, inProgressAtUtc, actor, null);

        var evt = new AlertInProgressEvent(
            EventId: new AlertEventId(Uuid7.New()),
            AlertId: AlertId,
            InProgressAtUtc: inProgressAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId);

        AddDomainEvent(evt);
    }

    public void Escalate(
        EscalationReason reason,
        UtcTimestamp escalatedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(AlertStatus.InProgress, AlertStatus.Acknowledged);

        var previous = Status;
        Status = AlertStatus.Escalated;
        EscalatedAtUtc = escalatedAtUtc;
        EscalatedBy = actor;
        EscalationReason = reason;

        AddHistory(previous, Status, escalatedAtUtc, actor, reason.Value);

        var evt = new AlertEscalatedEvent(
            EventId: new AlertEventId(Uuid7.New()),
            AlertId: AlertId,
            Reason: reason,
            EscalatedAtUtc: escalatedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId);

        AddDomainEvent(evt);
    }

    public void Resolve(
        ResolutionSummary summary,
        UtcTimestamp resolvedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(AlertStatus.InProgress, AlertStatus.Escalated);

        var previous = Status;
        Status = AlertStatus.Resolved;
        ResolvedAtUtc = resolvedAtUtc;
        ResolvedBy = actor;
        ResolutionSummary = summary;

        AddHistory(previous, Status, resolvedAtUtc, actor, summary.Value);

        var evt = new AlertResolvedEvent(
            EventId: new AlertEventId(Uuid7.New()),
            AlertId: AlertId,
            Summary: summary,
            ResolvedAtUtc: resolvedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId);

        AddDomainEvent(evt);
    }

    public void Close(
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(AlertStatus.Resolved);

        var previous = Status;
        Status = AlertStatus.Closed;
        ClosedAtUtc = closedAtUtc;
        ClosedBy = actor;

        AddHistory(previous, Status, closedAtUtc, actor, null);

        var evt = new AlertClosedEvent(
            EventId: new AlertEventId(Uuid7.New()),
            AlertId: AlertId,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId);

        AddDomainEvent(evt);
    }

    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(AlertStatus.Closed);

        var previous = Status;
        Status = AlertStatus.Archived;
        ArchivedAtUtc = archivedAtUtc;
        ArchivedBy = actor;

        AddHistory(previous, Status, archivedAtUtc, actor, null);

        var evt = new AlertArchivedEvent(
            EventId: new AlertEventId(Uuid7.New()),
            AlertId: AlertId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId);

        AddDomainEvent(evt);
    }

    public void AssignTo(
        string assignedTo,
        UtcTimestamp assignedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (string.IsNullOrWhiteSpace(assignedTo))
            throw new ArgumentException("AssignedTo is required.", nameof(assignedTo));

        var assignment = new AlertAssignment(
            alertAssignmentId: AlertAssignmentId.New(),
            alertId: AlertId,
            assignedTo: assignedTo.Trim(),
            assignedAtUtc: assignedAtUtc,
            assignedBy: actor.ToString());

        _assignments.Add(assignment);

        var evt = new AlertAssignedEvent(
            EventId: new AlertEventId(Uuid7.New()),
            AlertId: AlertId,
            AssignedTo: assignedTo.Trim(),
            AssignedAtUtc: assignedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId);

        AddDomainEvent(evt);
    }

    // ------------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------------

    private void EnsureStatusIsOneOf(params AlertStatus[] allowed)
    {
        foreach (var s in allowed)
        {
            if (Status == s)
                return;
        }

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Alert {AlertId}.");
    }

    private void AddHistory(
        AlertStatus previous,
        AlertStatus next,
        UtcTimestamp changedAtUtc,
        PerformedByActor actor,
        string? notes)
    {
        var entry = new AlertHistoryEntry(
            alertHistoryId: AlertHistoryId.New(),
            alertId: AlertId,
            previousStatus: previous,
            newStatus: next,
            changedAtUtc: changedAtUtc,
            changedBy: actor.ToString(),
            notes: notes);

        _history.Add(entry);
    }


    #region Snapshot Rebuild
    internal void RebuildFromSnapshot(AlertSnapshot snapshot)
    {
        // ------------------------------------------------------------
        // 1. Core identity + classification
        // ------------------------------------------------------------
        AlertId = snapshot.AlertId;
        AlertType = snapshot.AlertType;
        Severity = snapshot.Severity;
        Status = snapshot.Status;

        // ------------------------------------------------------------
        // 2. Core descriptive fields
        // ------------------------------------------------------------
        Title = snapshot.Title;
        Description = snapshot.Description;
        SourceModule = snapshot.SourceModule;

        // ------------------------------------------------------------
        // 3. Creation metadata
        // ------------------------------------------------------------
        CreatedAtUtc = snapshot.CreatedAtUtc;
        CreatedBy = snapshot.CreatedBy;

        // ------------------------------------------------------------
        // 4. Lifecycle timestamps
        // ------------------------------------------------------------
        AcknowledgedAtUtc = snapshot.AcknowledgedAtUtc;
        InProgressAtUtc = snapshot.InProgressAtUtc;
        EscalatedAtUtc = snapshot.EscalatedAtUtc;
        ResolvedAtUtc = snapshot.ResolvedAtUtc;
        ClosedAtUtc = snapshot.ClosedAtUtc;
        ArchivedAtUtc = snapshot.ArchivedAtUtc;

        // ------------------------------------------------------------
        // 5. SLA
        // ------------------------------------------------------------
        SlaExpiration = snapshot.SlaExpiration.HasValue
            ? new SlaExpiration(snapshot.SlaExpiration.Value)
            : null;

        // ------------------------------------------------------------
        // 6. Assignments
        // ------------------------------------------------------------
        _assignments.Clear();
        foreach (var assignment in snapshot.Assignments)
        {
            _assignments.Add(assignment);
        }

        // ------------------------------------------------------------
        // 7. Metadata
        // ------------------------------------------------------------
        _metadata.Clear();
        foreach (var entry in snapshot.Metadata)
        {
            _metadata.Add(entry);
        }

        // ------------------------------------------------------------
        // 8. History
        // ------------------------------------------------------------
        _history.Clear();
        foreach (var entry in snapshot.History)
        {
            _history.Add(entry);
        }

        // ------------------------------------------------------------
        // 9. Domain events must be cleared
        // ------------------------------------------------------------
        ClearDomainEvents();
    }


    #endregion



    #region Rehydration Apply Methods
    internal void ApplyCreated(AlertCreatedEvent e)
    {
        AlertId = e.AlertId;
        AlertType = e.AlertType;
        Severity = e.Severity;
        Status = AlertStatus.Created;
        Title = e.Title ?? Title;
        Description = e.Description ?? Description;
        SourceModule = e.SourceModule;
        CreatedAtUtc = e.CreatedAtUtc;
        // Actor.UserId is only populated for human actors (ActorKind.User);
        // system/AI/external-originated alerts carry no UserId, so fall back
        // to the current (default/empty) value rather than throw during replay.
        CreatedBy = e.Actor.UserId ?? CreatedBy;

        // SLA may be set later by factory or policy
    }

    internal void ApplyAcknowledged(AlertAcknowledgedEvent e)
    {
        Status = AlertStatus.Acknowledged;
        AcknowledgedAtUtc = e.AcknowledgedAtUtc;
        AcknowledgedBy = e.Actor;
    }

    internal void ApplyInProgress(AlertInProgressEvent e)
    {
        Status = AlertStatus.InProgress;
        InProgressAtUtc = e.InProgressAtUtc;
        InProgressBy = e.Actor;
    }

    internal void ApplyEscalated(AlertEscalatedEvent e)
    {
        Status = AlertStatus.Escalated;
        EscalatedAtUtc = e.EscalatedAtUtc;
        EscalatedBy = e.Actor;
        EscalationReason = e.Reason;
    }

    internal void ApplyResolved(AlertResolvedEvent e)
    {
        Status = AlertStatus.Resolved;
        ResolvedAtUtc = e.ResolvedAtUtc;
        ResolvedBy = e.Actor;
        ResolutionSummary = e.Summary;
    }

    internal void ApplyClosed(AlertClosedEvent e)
    {
        Status = AlertStatus.Closed;
        ClosedAtUtc = e.ClosedAtUtc;
        ClosedBy = e.Actor;
    }

    internal void ApplyArchived(AlertArchivedEvent e)
    {
        Status = AlertStatus.Archived;
        ArchivedAtUtc = e.ArchivedAtUtc;
        ArchivedBy = e.Actor;
    }

    internal void ApplyAssigned(AlertAssignedEvent e)
    {
        var assignment = new AlertAssignment(
            alertAssignmentId: AlertAssignmentId.New(),
            alertId: AlertId,
            assignedTo: e.AssignedTo,
            assignedAtUtc: e.AssignedAtUtc,
            assignedBy: e.Actor.ToString());

        _assignments.Add(assignment);
    }

    internal void ApplyMetadataAdded(AlertMetadataAddedEvent e)
    {
        var entry = new AlertMetadataEntry(
            alertMetadataId: AlertMetadataId.New(),
            alertId: AlertId,
            key: e.Key,
            value: e.Value);

        _metadata.Add(entry);
    }


    #endregion


    internal void ApplyMetadataUpdated(AlertMetadataUpdatedEvent e)
    {
        // AlertMetadataEntry is an immutable entity (ValueObject Rule 2.8),
        // so updates are applied by replacing the entry rather than mutating
        // it in place.
        var index = _metadata.FindIndex(m => m.Key == e.Key);
        if (index >= 0)
        {
            var existing = _metadata[index];
            _metadata[index] = new AlertMetadataEntry(
                alertMetadataId: existing.AlertMetadataId,
                alertId: existing.AlertId,
                key: existing.Key,
                value: e.NewValue);
        }
    }

    internal void ApplyMetadataRemoved(AlertMetadataRemovedEvent e)
    {
        var existing = _metadata.FirstOrDefault(m => m.Key == e.Key);
        if (existing is null)
            return;

        _metadata.Remove(existing);
    }


    private void AddDomainEvent(IAlertDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
