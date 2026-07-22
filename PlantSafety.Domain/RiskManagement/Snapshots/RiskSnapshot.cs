// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           RiskSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Risk aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.RiskManagement.Enums;
using PlantSafety.Domain.RiskManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.RiskManagement.Snapshots;

/// <summary>Immutable snapshot of a Risk aggregate for fast rehydration.</summary>
public sealed record RiskSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public RiskId RiskId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public RiskStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}