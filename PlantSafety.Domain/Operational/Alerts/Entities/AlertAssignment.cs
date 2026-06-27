// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertAssignment.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Immutable entity representing an assignment event for an Alert.
//      Complies with:
//          - StrongId Enforcement Rule (5.5)
//          - Deterministic Time Rule (1.1)
//          - ValueObject Rule (2.8)
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Entities;

public sealed record AlertAssignment
{
    public AlertAssignmentId AlertAssignmentId { get; }
    public AlertId AlertId { get; }
    public string AssignedTo { get; }
    public UtcTimestamp AssignedAtUtc { get; }
    public string AssignedBy { get; }

    public AlertAssignment(
        AlertAssignmentId alertAssignmentId,
        AlertId alertId,
        string assignedTo,
        UtcTimestamp assignedAtUtc,
        string assignedBy)
    {
        if (string.IsNullOrWhiteSpace(assignedTo))
            throw new ArgumentException("AssignedTo is required.", nameof(assignedTo));

        if (string.IsNullOrWhiteSpace(assignedBy))
            throw new ArgumentException("AssignedBy is required.", nameof(assignedBy));

        AlertAssignmentId = alertAssignmentId;
        AlertId = alertId;
        AssignedTo = assignedTo.Trim();
        AssignedAtUtc = assignedAtUtc;
        AssignedBy = assignedBy.Trim();
    }
}
