// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SensorIntegrityCheck
//  Layer:          Domain
//  File:           SensorIntegrityCheckEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for SensorIntegrityCheck domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for SensorIntegrityCheck domain events.</summary>
public readonly record struct SensorIntegrityCheckEventId(Guid Value)
{
    /// <summary>Creates a new SensorIntegrityCheckEventId using Uuid7.</summary>
    public static SensorIntegrityCheckEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
