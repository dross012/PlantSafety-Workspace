// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessEvents
//  Layer:          Domain
//  File:           AccessEventType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Classification of the raw badge-system event outcome.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Enums;

/// <summary>Classification of the raw badge-system event outcome.</summary>
public enum AccessEventType
{
    /// <summary>Access was granted to the presented credential.</summary>
    Granted = 0,

    /// <summary>Access was denied (invalid, suspended, or revoked credential).</summary>
    Denied = 1,

    /// <summary>A second, uncredentialed person entered immediately behind a credentialed holder.</summary>
    Tailgate = 2,

    /// <summary>The access point was physically forced open without a valid credential presentation.</summary>
    ForcedEntry = 3,

    /// <summary>Access occurred outside the zone's/policy's approved hours.</summary>
    AfterHours = 4,
}
