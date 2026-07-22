// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           AccidentSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Accident aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.AccidentManagement.Enums;
using PlantSafety.Domain.AccidentManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.AccidentManagement.Snapshots;

/// <summary>Immutable snapshot of a Accident aggregate for fast rehydration.</summary>
public sealed record AccidentSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public AccidentId AccidentId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public AccidentStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}