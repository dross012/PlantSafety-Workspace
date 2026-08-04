// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           MutualAidAgreementId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for the MutualAidAgreement child entity.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a <see cref="Entities.MutualAidAgreement"/> child entity.</summary>
public readonly record struct MutualAidAgreementId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a MutualAidAgreementId wrapping a non-empty GUID.</summary>
    public MutualAidAgreementId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MutualAidAgreementId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new MutualAidAgreementId using Uuid7.</summary>
    public static MutualAidAgreementId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a MutualAidAgreementId.</summary>
    public static MutualAidAgreementId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
