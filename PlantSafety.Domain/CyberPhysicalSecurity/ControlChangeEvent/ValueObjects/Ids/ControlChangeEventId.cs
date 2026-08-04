// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.ControlChangeEvent
//  Layer:          Domain
//  File:           ControlChangeEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for the ControlChangeEvent aggregate root (Anomalous
//                  Control-Change Detection subdomain of CyberPhysicalSecurity).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the ControlChangeEvent aggregate root.</summary>
public readonly record struct ControlChangeEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ControlChangeEventId wrapping a non-empty GUID.</summary>
    public ControlChangeEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ControlChangeEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ControlChangeEventId using Uuid7.</summary>
    public static ControlChangeEventId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ControlChangeEventId.</summary>
    public static ControlChangeEventId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
