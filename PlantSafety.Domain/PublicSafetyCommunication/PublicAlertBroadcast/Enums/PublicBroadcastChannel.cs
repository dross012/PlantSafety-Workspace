// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.PublicAlertBroadcast
//  Layer:          Domain
//  File:           PublicBroadcastChannel.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart enum modelling the public-facing broadcast channels used to warn the
//                  general public (as distinct from the agency-facing PublicSafetyDeliveryChannel
//                  used for IncidentNotification dispatch). Conceptually extends the
//                  AlertDeliveryChannel idea from the Alerts module to public-facing broadcast
//                  systems; standalone concept owned by this module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Enums;

/// <summary>Smart enum for public-facing broadcast channels (EAS/WEA/IPAWS and similar).</summary>
public sealed class PublicBroadcastChannel : IEquatable<PublicBroadcastChannel>
{
    /// <summary>Stable string code for persistence and API transport.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>Whether this channel supports a machine-readable delivery-confirmation callback.</summary>
    public bool SupportsDeliveryConfirmation { get; }

    private PublicBroadcastChannel(string code, string displayName, bool supportsDeliveryConfirmation)
    {
        Code = code;
        DisplayName = displayName;
        SupportsDeliveryConfirmation = supportsDeliveryConfirmation;
    }

    /// <summary>Emergency Alert System (broadcast TV/radio interruption).</summary>
    public static readonly PublicBroadcastChannel EmergencyAlertSystem = new("EmergencyAlertSystem", "Emergency Alert System (EAS)", supportsDeliveryConfirmation: false);

    /// <summary>Wireless Emergency Alerts (cell broadcast to mobile devices).</summary>
    public static readonly PublicBroadcastChannel WirelessEmergencyAlerts = new("WirelessEmergencyAlerts", "Wireless Emergency Alerts (WEA)", supportsDeliveryConfirmation: false);

    /// <summary>Integrated Public Alert and Warning System (federal aggregator).</summary>
    public static readonly PublicBroadcastChannel Ipaws = new("Ipaws", "IPAWS", supportsDeliveryConfirmation: true);

    /// <summary>Physical outdoor warning siren network.</summary>
    public static readonly PublicBroadcastChannel SirenNetwork = new("SirenNetwork", "Siren Network", supportsDeliveryConfirmation: true);

    /// <summary>Official social-media channel post.</summary>
    public static readonly PublicBroadcastChannel SocialMedia = new("SocialMedia", "Social Media", supportsDeliveryConfirmation: true);

    /// <summary>All defined channels.</summary>
    public static IReadOnlyCollection<PublicBroadcastChannel> All { get; } =
        new[] { EmergencyAlertSystem, WirelessEmergencyAlerts, Ipaws, SirenNetwork, SocialMedia };

    /// <summary>Default channel used when none is specified.</summary>
    public static PublicBroadcastChannel Default => Ipaws;

    /// <summary>Resolves a channel from its stable code. Throws if unknown.</summary>
    public static PublicBroadcastChannel From(string code) =>
        TryParse(code, out var value) ? value! : throw new ArgumentException($"Unknown PublicBroadcastChannel code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a channel from its stable code.</summary>
    public static bool TryParse(string? code, out PublicBroadcastChannel? value)
    {
        value = All.FirstOrDefault(c => string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase));
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognised channel code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(PublicBroadcastChannel? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as PublicBroadcastChannel);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
