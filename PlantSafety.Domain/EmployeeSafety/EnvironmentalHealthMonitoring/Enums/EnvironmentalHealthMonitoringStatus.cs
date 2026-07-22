// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
//  Layer:          Domain
//  File:           EnvironmentalHealthMonitoringStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real measurement lifecycle states for the EnvironmentalHealthMonitoring aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Enums;

/// <summary>Lifecycle states for the <see cref="EnvironmentalHealthMonitoring"/> aggregate root.</summary>
public enum EnvironmentalHealthMonitoringStatus
{
    /// <summary>Measurement scheduled.</summary>
    Scheduled = 0,

    /// <summary>Measurement recorded and within acceptable exposure limits.</summary>
    WithinLimits = 1,

    /// <summary>Measurement recorded an exceedance of the exposure limit.</summary>
    ExceedanceDetected = 2,

    /// <summary>Corrective action (ventilation fix, PPE upgrade, engineering control) is underway.</summary>
    CorrectiveActionInProgress = 3,

    /// <summary>Exceedance resolved and re-measurement confirmed compliance.</summary>
    Resolved = 4,
}
