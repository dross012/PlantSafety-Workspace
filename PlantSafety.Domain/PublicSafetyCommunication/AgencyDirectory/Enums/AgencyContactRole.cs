// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Domain
//  File:           AgencyContactRole.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Role a named contact plays within a PublicSafetyAgency's response structure.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Enums;

/// <summary>Role a named <see cref="AgencyContact"/> plays within its agency's response structure.</summary>
public enum AgencyContactRole
{
    /// <summary>Primary 24/7 dispatch/communications center.</summary>
    PrimaryDispatch = 0,

    /// <summary>On-scene incident commander or shift commander.</summary>
    IncidentCommander = 1,

    /// <summary>Designated liaison officer for cross-agency coordination.</summary>
    LiaisonOfficer = 2,

    /// <summary>Regulatory affairs / compliance contact.</summary>
    RegulatoryContact = 3,

    /// <summary>After-hours / on-call contact.</summary>
    AfterHours = 4,
}
