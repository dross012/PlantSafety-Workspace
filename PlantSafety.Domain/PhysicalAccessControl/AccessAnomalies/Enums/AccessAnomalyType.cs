// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessAnomalies
//  Layer:          Domain
//  File:           AccessAnomalyType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Classification of the detected anomaly condition.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Enums;

/// <summary>Classification of the detected anomaly condition.</summary>
public enum AccessAnomalyType
{
    /// <summary>A second, uncredentialed person entered immediately behind a credentialed holder.</summary>
    Tailgating = 0,

    /// <summary>An access point was physically forced open without a valid credential presentation.</summary>
    ForcedEntry = 1,

    /// <summary>Access occurred outside the zone's/policy's approved hours.</summary>
    AfterHoursAccess = 2,

    /// <summary>A credential without the required access level was used against a restricted zone.</summary>
    RestrictedZoneViolation = 3,

    /// <summary>An unrecognized, suspended, or revoked credential was presented.</summary>
    UnauthorizedAccess = 4,
}
