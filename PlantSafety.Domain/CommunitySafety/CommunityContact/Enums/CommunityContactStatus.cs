// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityContact
//  Layer:          Domain
//  File:           CommunityContactStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the CommunityContact aggregate root. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityContact.Enums;

/// <summary>Lifecycle states for the <see cref="Aggregates.CommunityContact"/> aggregate root.</summary>
public enum CommunityContactStatus
{
    /// <summary>Contact is registered and actively used for outreach/notifications. Initial state on Register.</summary>
    Active = 0,

    /// <summary>Contact is temporarily inactive (e.g. requested no further contact) but retained for records.</summary>
    Inactive = 1,

    /// <summary>Contact has been archived (soft-deleted) for long-term retention. Terminal state.</summary>
    Archived = 2,
}
