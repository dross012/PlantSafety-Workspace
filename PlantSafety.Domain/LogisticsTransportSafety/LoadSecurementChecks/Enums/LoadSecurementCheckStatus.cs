// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.LoadSecurementChecks
//  Layer:          Domain
//  File:           LoadSecurementCheckStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the LoadSecurementCheck aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Enums;

/// <summary>Lifecycle states for the <see cref="LoadSecurementCheck"/> aggregate root.</summary>
public enum LoadSecurementCheckStatus
{
    /// <summary>Check recorded with a passing outcome.</summary>
    Passed = 0,

    /// <summary>Check recorded with a failing outcome; corrective action required.</summary>
    Failed = 1,

    /// <summary>Corrective action for a failed check has been completed.</summary>
    CorrectiveActionCompleted = 2,
}
