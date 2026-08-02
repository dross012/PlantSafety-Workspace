// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Domain
//  File:           DigitalTwinLifecycleRules.cs
//  Created:        2026-08-02
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the DigitalTwin aggregate. Phase 4
//                  full build-out. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.Rules;

/// <summary>Defines permitted lifecycle transitions for the DigitalTwin aggregate.</summary>
public static class DigitalTwinLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(DigitalTwinStatus current) => current == DigitalTwinStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(DigitalTwinStatus current) => current == DigitalTwinStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(DigitalTwinStatus current) => current == DigitalTwinStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(DigitalTwinStatus current) => current == DigitalTwinStatus.Closed;
}
