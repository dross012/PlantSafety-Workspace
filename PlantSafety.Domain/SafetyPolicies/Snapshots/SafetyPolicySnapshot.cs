// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Domain
//  File:           SafetyPolicySnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for SafetyPolicy aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyPolicies.Enums;
using PlantSafety.Domain.SafetyPolicies.ValueObjects.Ids;

namespace PlantSafety.Domain.SafetyPolicies.Snapshots;

/// <summary>Immutable snapshot of a SafetyPolicy aggregate for fast rehydration.</summary>
public sealed record SafetyPolicySnapshot
{
    /// <summary>Aggregate identity.</summary>
    public SafetyPolicyId SafetyPolicyId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public SafetyPolicyStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}