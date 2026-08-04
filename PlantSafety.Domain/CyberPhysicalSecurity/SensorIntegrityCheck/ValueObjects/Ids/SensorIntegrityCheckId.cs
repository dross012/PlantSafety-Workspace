// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SensorIntegrityCheck
//  Layer:          Domain
//  File:           SensorIntegrityCheckId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for the SensorIntegrityCheck aggregate root (Sensor
//                  Integrity Monitoring subdomain of CyberPhysicalSecurity).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the SensorIntegrityCheck aggregate root.</summary>
public readonly record struct SensorIntegrityCheckId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a SensorIntegrityCheckId wrapping a non-empty GUID.</summary>
    public SensorIntegrityCheckId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SensorIntegrityCheckId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new SensorIntegrityCheckId using Uuid7.</summary>
    public static SensorIntegrityCheckId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a SensorIntegrityCheckId.</summary>
    public static SensorIntegrityCheckId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
