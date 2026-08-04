// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Domain
//  File:           AgencyOperationalStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle status of a PublicSafetyAgency directory record.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Enums;

/// <summary>Lifecycle status of a <see cref="PublicSafetyAgency"/> directory record.</summary>
public enum AgencyOperationalStatus
{
    /// <summary>Agency is registered and eligible to receive notifications.</summary>
    Active = 0,

    /// <summary>Agency has been deactivated and will no longer be dispatched notifications.</summary>
    Inactive = 1,
}
