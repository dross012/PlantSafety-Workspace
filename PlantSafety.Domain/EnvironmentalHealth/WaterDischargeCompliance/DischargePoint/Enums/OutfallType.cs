// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePoint
//  Layer:          Domain
//  File:           OutfallType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Classification of a discharge point outfall configuration. Additive-only.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Enums;

/// <summary>Classification of a discharge point outfall configuration.</summary>
public enum OutfallType
{
    /// <summary>Direct discharge to a receiving water body.</summary>
    Direct = 0,

    /// <summary>Indirect discharge through a publicly owned treatment works.</summary>
    Indirect = 1,

    /// <summary>Combined stormwater/process discharge outfall.</summary>
    StormwaterCombined = 2,
}