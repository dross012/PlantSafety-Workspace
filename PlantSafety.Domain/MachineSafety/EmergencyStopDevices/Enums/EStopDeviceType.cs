// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.EmergencyStopDevices
//  Layer:          Domain
//  File:           EStopDeviceType.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Classification of Emergency Stop (E-Stop) device types per the PlantSafety Program
//                  Structure Emergency Stop Devices component.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Enums;

/// <summary>Classification of Emergency Stop (E-Stop) device types.</summary>
public enum EStopDeviceType
{
    /// <summary>Large, red push-button stop located near an operator station.</summary>
    PushButtonStop = 0,

    /// <summary>Pull-cord system along a conveyor line.</summary>
    PullCordSystem = 1,

    /// <summary>Cable-actuated stop for long or complex machinery.</summary>
    CableActuatedStop = 2,
}
