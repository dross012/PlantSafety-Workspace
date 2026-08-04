// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessPoints
//  Layer:          Domain
//  File:           AccessPointDeviceType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Physical device classification for an AccessPoint.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Enums;

/// <summary>Physical device classification for an <see cref="AccessPoint"/>.</summary>
public enum AccessPointDeviceType
{
    /// <summary>Standalone badge reader (door or cabinet).</summary>
    BadgeReader = 0,

    /// <summary>Single-person turnstile.</summary>
    Turnstile = 1,

    /// <summary>Interlocked mantrap door pair (anti-tailgating).</summary>
    MantrapDoor = 2,

    /// <summary>Vehicle gate at a site perimeter or yard.</summary>
    VehicleGate = 3,

    /// <summary>Pedestrian perimeter gate.</summary>
    PerimeterGate = 4,
}
