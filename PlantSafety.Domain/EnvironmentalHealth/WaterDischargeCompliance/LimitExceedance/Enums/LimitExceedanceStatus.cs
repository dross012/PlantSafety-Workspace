// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
//  Layer:          Domain
//  File:           LimitExceedanceStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the LimitExceedance aggregate root. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Enums;

/// <summary>Lifecycle states for the <see cref="LimitExceedance"/> aggregate root.</summary>
public enum LimitExceedanceStatus
{
    /// <summary>Exceedance detected, response not yet initiated.</summary>
    Detected = 0,

    /// <summary>Exceedance response workflow has been initiated.</summary>
    ResponseInitiated = 1,

    /// <summary>Exceedance response closed out.</summary>
    Closed = 2,
}