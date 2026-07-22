// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertDeliveryRecord.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Entity recording a single multi-channel delivery attempt for an Alert, per
//                  "12. Multi-Channel Alert Delivery" and "Multi-Channel Delivery Status Table"
//                  (Alerts & Emergency Response Dashboard).
//                  Additive-only evolution.
// ============================================================================
using AlertDeliveryChannel = PlantSafety.Domain.Alerts.Alert.Enums.AlertDeliveryChannel;
using AlertId = PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids.AlertId;

namespace PlantSafety.Domain.Alerts.Alert.Entities;

/// <summary>Records a single delivery attempt of an <see cref="Aggregates.Alert"/> over one channel.</summary>
public sealed class AlertDeliveryRecord
{
    /// <summary>Unique identity of this delivery record.</summary>
    public Guid DeliveryRecordId { get; private set; }

    /// <summary>Identity of the parent Alert aggregate.</summary>
    public AlertId AlertId { get; private set; }

    /// <summary>Delivery channel used.</summary>
    public AlertDeliveryChannel Channel { get; private set; }

    /// <summary>Whether the delivery attempt succeeded.</summary>
    public bool Succeeded { get; private set; }

    /// <summary>UTC instant the delivery attempt was made.</summary>
    public UtcTimestamp AttemptedAtUtc { get; private set; }

    /// <summary>Optional sanitized failure reason, when <see cref="Succeeded"/> is <see langword="false"/>.</summary>
    public string? FailureReason { get; private set; }

    /// <summary>For rehydration only.</summary>
    private AlertDeliveryRecord() { }

    /// <summary>Creates a new delivery record.</summary>
    public AlertDeliveryRecord(
        Guid deliveryRecordId,
        AlertId alertId,
        AlertDeliveryChannel channel,
        bool succeeded,
        UtcTimestamp attemptedAtUtc,
        string? failureReason = null)
    {
        DeliveryRecordId = deliveryRecordId;
        AlertId          = alertId;
        Channel          = channel;
        Succeeded        = succeeded;
        AttemptedAtUtc   = attemptedAtUtc;
        FailureReason    = succeeded ? null : (string.IsNullOrWhiteSpace(failureReason) ? "Unknown" : failureReason.Trim());
    }
}
