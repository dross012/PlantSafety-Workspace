// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitTemplate
//  Layer:          Domain
//  File:           PermitTemplateSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for PermitTemplate aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitTemplate.Snapshots;

/// <summary>Immutable snapshot of a <see cref="PermitTemplate"/> aggregate for fast rehydration.</summary>
public sealed record PermitTemplateSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public PermitTemplateId PermitTemplateId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public PermitTemplateStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}