// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertDeliveryChannel.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Simple classification enum for "12. Multi-Channel Alert Delivery" methods
//                  from the source doc. Pure vocabulary with no behavior — satisfies the Simple
//                  Enum branch of the Enums Rule (2.7).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Alerts.Alert.Enums;

/// <summary>Identifies a delivery method used to notify recipients of an <see cref="Aggregates.Alert"/>.</summary>
public enum AlertDeliveryChannel
{
    /// <summary>High-decibel intelligible audio PA speaker announcement.</summary>
    PaSpeaker = 0,

    /// <summary>Strobe light visual alerting.</summary>
    StrobeLight = 1,

    /// <summary>Digital signage display.</summary>
    DigitalSignage = 2,

    /// <summary>SMS/text message.</summary>
    Sms = 3,

    /// <summary>Mobile push notification.</summary>
    MobilePush = 4,

    /// <summary>Wearable device vibration alert.</summary>
    WearableVibration = 5,

    /// <summary>Email alert.</summary>
    Email = 6,

    /// <summary>SCADA/HMI pop-up alert.</summary>
    ScadaHmiPopup = 7,

    /// <summary>Rave Mobile Safety platform integration.</summary>
    RaveMobileSafety = 8,

    /// <summary>PlantSafety dashboard in-app alert.</summary>
    PlantSafetyDashboard = 9,
}
