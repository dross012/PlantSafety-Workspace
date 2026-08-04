// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit
//  Layer:          Domain
//  File:           DischargeLimitStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the DischargeLimit aggregate root. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Enums;

/// <summary>Lifecycle states for the <see cref="DischargeLimit"/> aggregate root.</summary>
public enum DischargeLimitStatus
{
    /// <summary>Limit is in force.</summary>
    Active = 0,

    /// <summary>Limit has been retired (superseded or permit condition removed).</summary>
    Retired = 1,
}