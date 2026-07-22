// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertRoutingService.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Pure domain service resolving which delivery channels an Alert should be
//                  routed to, based on severity and category, per "12. Multi-Channel Alert
//                  Delivery: Alerts must cut through noise, PPE, and distance". Contains no
//                  infrastructure logic (no HTTP/SQL/messaging) — the Infrastructure layer is
//                  responsible for actually dispatching to the resolved channels.
//                  Additive-only evolution.
// ============================================================================
using AlertSeverity = PlantSafety.Domain.Alerts.Alert.Enums.AlertSeverity;
using AlertDeliveryChannel = PlantSafety.Domain.Alerts.Alert.Enums.AlertDeliveryChannel;

namespace PlantSafety.Domain.Alerts.Alert.Services;

/// <summary>Domain service resolving the canonical delivery-channel set for an alert severity.</summary>
public sealed class AlertRoutingService : IDomainService
{
    /// <summary>
    /// Resolves the ordered set of delivery channels an alert of the given severity must be
    /// routed to. Critical/High severities cut through with the widest channel fan-out; Low
    /// severity is limited to passive channels.
    /// </summary>
    public IReadOnlyList<AlertDeliveryChannel> ResolveChannels(AlertSeverity severity)
    {
        ArgumentNullException.ThrowIfNull(severity);

        if (severity.Equals(AlertSeverity.Critical))
            return new[]
            {
                AlertDeliveryChannel.PaSpeaker,
                AlertDeliveryChannel.StrobeLight,
                AlertDeliveryChannel.DigitalSignage,
                AlertDeliveryChannel.Sms,
                AlertDeliveryChannel.MobilePush,
                AlertDeliveryChannel.WearableVibration,
                AlertDeliveryChannel.ScadaHmiPopup,
                AlertDeliveryChannel.RaveMobileSafety,
                AlertDeliveryChannel.PlantSafetyDashboard,
            };

        if (severity.Equals(AlertSeverity.High))
            return new[]
            {
                AlertDeliveryChannel.DigitalSignage,
                AlertDeliveryChannel.Sms,
                AlertDeliveryChannel.MobilePush,
                AlertDeliveryChannel.ScadaHmiPopup,
                AlertDeliveryChannel.PlantSafetyDashboard,
            };

        if (severity.Equals(AlertSeverity.Medium))
            return new[]
            {
                AlertDeliveryChannel.MobilePush,
                AlertDeliveryChannel.PlantSafetyDashboard,
            };

        return new[] { AlertDeliveryChannel.Email, AlertDeliveryChannel.PlantSafetyDashboard };
    }
}
