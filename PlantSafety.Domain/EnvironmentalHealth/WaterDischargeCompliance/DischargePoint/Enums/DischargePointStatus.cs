// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePoint
//  Layer:          Domain
//  File:           DischargePointStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the DischargePoint aggregate root. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Enums;

/// <summary>Lifecycle states for the <see cref="DischargePoint"/> aggregate root.</summary>
public enum DischargePointStatus
{
    /// <summary>Discharge point registered and actively discharging under permit.</summary>
    Active = 0,

    /// <summary>Discharge point temporarily inactive.</summary>
    Inactive = 1,

    /// <summary>Discharge point permanently decommissioned.</summary>
    Decommissioned = 2,
}