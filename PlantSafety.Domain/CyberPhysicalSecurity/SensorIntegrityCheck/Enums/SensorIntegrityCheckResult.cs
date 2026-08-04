// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SensorIntegrityCheck
//  Layer:          Domain
//  File:           SensorIntegrityCheckResult.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Outcome of a sensor-integrity check. Drives the "sensor integrity pass rate" KPI.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Enums;

/// <summary>Outcome of a sensor-integrity check.</summary>
public enum SensorIntegrityCheckResult
{
    /// <summary>Sensor integrity confirmed.</summary>
    Pass = 0,

    /// <summary>Sensor integrity check failed.</summary>
    Fail = 1,

    /// <summary>Check could not be completed conclusively.</summary>
    Inconclusive = 2
}
