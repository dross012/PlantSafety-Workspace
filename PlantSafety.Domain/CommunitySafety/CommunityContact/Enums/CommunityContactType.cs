// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityContact
//  Layer:          Domain
//  File:           CommunityContactType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Pure classification of the kind of community relationship a CommunityContact
//                  represents. No behavior/metadata beyond the label itself, so this is a simple
//                  enum per the Enums Rule (2026 Revision) rather than a Smart Enum. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityContact.Enums;

/// <summary>Classification of the community relationship represented by a <see cref="Aggregates.CommunityContact"/>.</summary>
public enum CommunityContactType
{
    /// <summary>A resident living near the facility.</summary>
    Resident = 0,

    /// <summary>An elected or appointed local government official.</summary>
    LocalOfficial = 1,

    /// <summary>A local emergency services contact (fire, police, EMS liaison).</summary>
    EmergencyServices = 2,

    /// <summary>A media/press contact.</summary>
    Media = 3,

    /// <summary>A community advocacy or neighborhood association group.</summary>
    CommunityGroup = 4,

    /// <summary>A regulatory agency contact (e.g. environmental or right-to-know liaison).</summary>
    RegulatoryAgency = 5,
}
