// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.IncidentNotificationDispatch
//  Layer:          Domain
//  File:           PublicSafetyDeliveryChannel.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart enum modelling the outbound delivery channels used to notify external
//                  public-safety agencies (fire, police, EMS, regulatory hotlines). Conceptually
//                  extends the AlertDeliveryChannel idea from the Alerts module to
//                  external/public-facing channels; this is a standalone concept owned by this
//                  module (the Alerts module is not modified). Carries workflow metadata
//                  (acknowledgement requirement, real-time expectation, category) per the
//                  Enums Rule (2.7): Smart Enums are required when a type carries business
//                  rules/metadata and drives workflow.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Enums;

/// <summary>Smart enum for outbound delivery channels used to notify external public-safety agencies.</summary>
public sealed class PublicSafetyDeliveryChannel : IEquatable<PublicSafetyDeliveryChannel>
{
    /// <summary>Stable string code for persistence and API transport.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>Whether this channel is expected to produce an explicit human acknowledgement.</summary>
    public bool RequiresAcknowledgement { get; }

    /// <summary>Whether this channel is a real-time/synchronous communication path.</summary>
    public bool IsRealTime { get; }

    private PublicSafetyDeliveryChannel(string code, string displayName, bool requiresAcknowledgement, bool isRealTime)
    {
        Code = code;
        DisplayName = displayName;
        RequiresAcknowledgement = requiresAcknowledgement;
        IsRealTime = isRealTime;
    }

    /// <summary>Direct telephone call to the agency's dispatch/contact line.</summary>
    public static readonly PublicSafetyDeliveryChannel Phone = new("Phone", "Telephone", requiresAcknowledgement: true, isRealTime: true);

    /// <summary>Facsimile transmission (still required by some regulatory hotlines).</summary>
    public static readonly PublicSafetyDeliveryChannel Fax = new("Fax", "Facsimile", requiresAcknowledgement: false, isRealTime: false);

    /// <summary>Email notification.</summary>
    public static readonly PublicSafetyDeliveryChannel Email = new("Email", "Email", requiresAcknowledgement: false, isRealTime: false);

    /// <summary>SMS/text message notification.</summary>
    public static readonly PublicSafetyDeliveryChannel Sms = new("Sms", "SMS", requiresAcknowledgement: false, isRealTime: true);

    /// <summary>Two-way radio dispatch transmission.</summary>
    public static readonly PublicSafetyDeliveryChannel RadioDispatch = new("RadioDispatch", "Radio Dispatch", requiresAcknowledgement: true, isRealTime: true);

    /// <summary>Computer-Aided Dispatch (CAD) system-to-system integration.</summary>
    public static readonly PublicSafetyDeliveryChannel CadIntegration = new("CadIntegration", "CAD Integration", requiresAcknowledgement: true, isRealTime: true);

    /// <summary>In-person liaison notification (on-site or agency office).</summary>
    public static readonly PublicSafetyDeliveryChannel InPersonLiaison = new("InPersonLiaison", "In-Person Liaison", requiresAcknowledgement: true, isRealTime: true);

    /// <summary>All defined channels.</summary>
    public static IReadOnlyCollection<PublicSafetyDeliveryChannel> All { get; } =
        new[] { Phone, Fax, Email, Sms, RadioDispatch, CadIntegration, InPersonLiaison };

    /// <summary>Default channel used when none is specified.</summary>
    public static PublicSafetyDeliveryChannel Default => Phone;

    /// <summary>Resolves a channel from its stable code. Throws if unknown.</summary>
    public static PublicSafetyDeliveryChannel From(string code) =>
        TryParse(code, out var value) ? value! : throw new ArgumentException($"Unknown PublicSafetyDeliveryChannel code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a channel from its stable code.</summary>
    public static bool TryParse(string? code, out PublicSafetyDeliveryChannel? value)
    {
        value = All.FirstOrDefault(c => string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase));
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognised channel code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(PublicSafetyDeliveryChannel? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as PublicSafetyDeliveryChannel);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
