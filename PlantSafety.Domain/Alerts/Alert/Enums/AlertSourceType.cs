// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertSourceType.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Simple classification enum for "Trigger source (sensor, CV, human)" per
//                  "14. Alert Logging & Reporting". Pure vocabulary with no behavior — satisfies
//                  the Simple Enum branch of the Enums Rule (2.7).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Alerts.Alert.Enums;

/// <summary>Identifies the origin that triggered an <see cref="Aggregates.Alert"/>.</summary>
public enum AlertSourceType
{
    /// <summary>Triggered by an IoT/environmental/equipment sensor.</summary>
    Sensor = 0,

    /// <summary>Triggered by a computer-vision/observability detection model.</summary>
    ComputerVision = 1,

    /// <summary>Triggered by a wearable device (fall detection, heat stress, vibration).</summary>
    Wearable = 2,

    /// <summary>Triggered by manual SOS/panic button activation.</summary>
    PanicButton = 3,

    /// <summary>Reported directly by a human observer (supervisor, operator, security).</summary>
    Human = 4,

    /// <summary>Triggered by an automated plant system (SCADA/HMI, PLC, building management system).</summary>
    System = 5,
}
