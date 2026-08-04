// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SensorIntegrityCheck
//  Layer:          Domain
//  File:           SensorIntegrityCheckType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Category of sensor-integrity verification performed.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Enums;

/// <summary>Category of sensor-integrity verification performed.</summary>
public enum SensorIntegrityCheckType
{
    /// <summary>Calibration accuracy check against a reference standard.</summary>
    Calibration = 0,

    /// <summary>Signal-plausibility / signal-injection tamper check.</summary>
    Signal = 1,

    /// <summary>Firmware/software integrity (hash/signature) check.</summary>
    Firmware = 2,

    /// <summary>Physical tamper-evidence check (enclosure, seal, wiring).</summary>
    Tamper = 3
}
