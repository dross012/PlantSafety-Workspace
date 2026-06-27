// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertHistoryEntry.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Immutable entity representing a single lifecycle transition for an
//      Alert aggregate. Complies with:
//          - StrongId Enforcement Rule (5.5)
//          - Deterministic Time Rule (1.1)
//          - ValueObject Rule (2.8)
//          - Canonical Aggregate Rule (2.5)
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Entities;

public sealed record AlertHistoryEntry
{
    public AlertHistoryId AlertHistoryId { get; }
    public AlertId AlertId { get; }
    public AlertStatus PreviousStatus { get; }
    public AlertStatus NewStatus { get; }
    public UtcTimestamp ChangedAtUtc { get; }
    public string ChangedBy { get; }
    public string? Notes { get; }

    public AlertHistoryEntry(
        AlertHistoryId alertHistoryId,
        AlertId alertId,
        AlertStatus previousStatus,
        AlertStatus newStatus,
        UtcTimestamp changedAtUtc,
        string changedBy,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(changedBy))
            throw new ArgumentException("ChangedBy is required.", nameof(changedBy));

        AlertHistoryId = alertHistoryId;
        AlertId = alertId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        ChangedAtUtc = changedAtUtc;
        ChangedBy = changedBy.Trim();
        Notes = notes?.Trim();
    }
}
