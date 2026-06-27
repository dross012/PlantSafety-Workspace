// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.RiskManagement
//  Layer:          Domain
//  File:           RiskManagementSnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for RiskManagement aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.RiskManagement.Snapshots;

/// <summary>Immutable snapshot of a <see cref="RiskManagement"/> aggregate for fast rehydration.</summary>
public sealed record RiskManagementSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public RiskManagementId RiskManagementId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public RiskManagementStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
