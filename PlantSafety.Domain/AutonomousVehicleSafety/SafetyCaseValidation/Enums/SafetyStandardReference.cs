// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.SafetyCaseValidation
//  Layer:          Domain
//  File:           SafetyStandardReference.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Regulatory/industry standard a safety case is validated against.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Enums;

/// <summary>Regulatory/industry standard a safety case is validated against.</summary>
public enum SafetyStandardReference
{
    /// <summary>ANSI/RIA R15.08 (mobile robot safety).</summary>
    AnsiRia1508 = 0,

    /// <summary>ISO 3691-4 (industrial trucks — driverless AGV safety requirements).</summary>
    Iso36914 = 1,

    /// <summary>Another applicable standard not separately enumerated.</summary>
    Other = 2,
}
