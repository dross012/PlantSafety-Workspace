// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           JointResponsePlanId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for the JointResponsePlan child entity.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a <see cref="Entities.JointResponsePlan"/> child entity.</summary>
public readonly record struct JointResponsePlanId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a JointResponsePlanId wrapping a non-empty GUID.</summary>
    public JointResponsePlanId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("JointResponsePlanId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new JointResponsePlanId using Uuid7.</summary>
    public static JointResponsePlanId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a JointResponsePlanId.</summary>
    public static JointResponsePlanId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
