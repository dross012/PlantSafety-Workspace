// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Domain
//  File:           EmergencyManagementSnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for EmergencyManagement aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.EmergencyManagement.Snapshots;

/// <summary>Immutable snapshot of a <see cref="EmergencyManagement"/> aggregate for fast rehydration.</summary>
public sealed record EmergencyManagementSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public EmergencyManagementId EmergencyManagementId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public EmergencyManagementStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
