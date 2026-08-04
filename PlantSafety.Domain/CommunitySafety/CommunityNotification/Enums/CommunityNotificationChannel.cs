// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityNotification
//  Layer:          Domain
//  File:           CommunityNotificationChannel.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Pure classification of the channel a community notification was sent through.
//                  No behavior/metadata beyond the label itself, so this is a simple enum per the
//                  Enums Rule (2026 Revision). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityNotification.Enums;

/// <summary>Classification of the delivery channel for a <see cref="Aggregates.CommunityNotification"/>.</summary>
public enum CommunityNotificationChannel
{
    /// <summary>Delivered via email.</summary>
    Email = 0,

    /// <summary>Delivered via SMS text message.</summary>
    Sms = 1,

    /// <summary>Delivered via a phone call.</summary>
    PhoneCall = 2,

    /// <summary>Delivered via postal mail.</summary>
    PostalMail = 3,

    /// <summary>Delivered via a public posting (e.g. community website, social media, physical notice).</summary>
    PublicPosting = 4,

    /// <summary>Delivered via a future/external public-safety communication platform (integration point for a PublicSafetyCommunication module).</summary>
    ExternalPublicSafetyPlatform = 5,
}
