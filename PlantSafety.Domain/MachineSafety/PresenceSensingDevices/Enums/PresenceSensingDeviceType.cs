// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.PresenceSensingDevices
//  Layer:          Domain
//  File:           PresenceSensingDeviceType.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Classification of presence-sensing device types per the PlantSafety Program
//                  Structure Presence-Sensing Devices component.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Enums;

/// <summary>Classification of presence-sensing device types.</summary>
public enum PresenceSensingDeviceType
{
    /// <summary>Light curtain detecting interruptions in infrared beams.</summary>
    LightCurtain = 0,

    /// <summary>Safety laser scanner monitoring a defined protective field.</summary>
    SafetyLaserScanner = 1,

    /// <summary>Pressure-sensitive mat triggering shutdown when stepped on.</summary>
    PressureSensitiveMat = 2,
}
