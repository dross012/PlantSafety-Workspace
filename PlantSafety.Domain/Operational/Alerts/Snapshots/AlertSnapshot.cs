// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertSnapshot.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Immutable snapshot representation of the Alert aggregate. Used for:
//          - Rebuild pipelines
//          - Replay pipelines
//          - Analytics pipelines
//          - Outbox consumers
//          - Deterministic serialization
//
//      Complies with:
//          - Canonical Snapshot Rule (2.15)
//          - Replay & Rebuild Rule
//          - Deterministic Behavior Rule (1.3)
//          - Additive-only evolution
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.Snapshots;

public sealed class AlertSnapshot
{
    public AlertId AlertId { get; }
    public AlertType AlertType { get; }
    public AlertSeverity Severity { get; }
    public AlertStatus Status { get; }

    public string Title { get; }
    public string Description { get; }
    public string SourceModule { get; }

    public UtcTimestamp CreatedAtUtc { get; }
    public UserId CreatedBy { get; }

    public UtcTimestamp? AcknowledgedAtUtc { get; }
    public UtcTimestamp? InProgressAtUtc { get; }
    public UtcTimestamp? EscalatedAtUtc { get; }
    public UtcTimestamp? ResolvedAtUtc { get; }
    public UtcTimestamp? ClosedAtUtc { get; }
    public UtcTimestamp? ArchivedAtUtc { get; }

    public UtcTimestamp? SlaExpiration { get; }

    public IReadOnlyList<AlertAssignment> Assignments { get; }
    public IReadOnlyList<AlertMetadataEntry> Metadata { get; }
    public IReadOnlyList<AlertHistoryEntry> History { get; }

    public AlertSnapshot(
        AlertId alertId,
        AlertType type,
        AlertSeverity severity,
        AlertStatus status,
        string title,
        string description,
        string sourceModule,
        UtcTimestamp createdAtUtc,
        UserId createdBy,
        UtcTimestamp? acknowledgedAtUtc,
        UtcTimestamp? inProgressAtUtc,
        UtcTimestamp? escalatedAtUtc,
        UtcTimestamp? resolvedAtUtc,
        UtcTimestamp? closedAtUtc,
        UtcTimestamp? archivedAtUtc,
        UtcTimestamp? slaExpiration,
        IReadOnlyList<AlertAssignment> assignments,
        IReadOnlyList<AlertMetadataEntry> metadata,
        IReadOnlyList<AlertHistoryEntry> history)
    {
        AlertId = alertId;
        AlertType = type;
        Severity = severity;
        Status = status;

        Title = title;
        Description = description;
        SourceModule = sourceModule;

        CreatedAtUtc = createdAtUtc;
        CreatedBy = createdBy;

        AcknowledgedAtUtc = acknowledgedAtUtc;
        InProgressAtUtc = inProgressAtUtc;
        EscalatedAtUtc = escalatedAtUtc;
        ResolvedAtUtc = resolvedAtUtc;
        ClosedAtUtc = closedAtUtc;
        ArchivedAtUtc = archivedAtUtc;

        SlaExpiration = slaExpiration;

        Assignments = assignments;
        Metadata = metadata;
        History = history;
    }
}
