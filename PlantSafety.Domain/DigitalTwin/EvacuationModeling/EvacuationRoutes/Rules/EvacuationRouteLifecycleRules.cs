// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Domain
//  File:           EvacuationRouteLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the EvacuationRoute aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.EvacuationRoute"/> aggregate.</summary>
public static class EvacuationRouteLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when an obstruction may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordObstruction(EvacuationRouteStatus current) =>
        current is EvacuationRouteStatus.Active or EvacuationRouteStatus.Obstructed;

    /// <summary>Returns <see langword="true"/> when an obstruction may be cleared from <paramref name="current"/>.</summary>
    public static bool CanClearObstruction(EvacuationRouteStatus current) => current == EvacuationRouteStatus.Obstructed;

    /// <summary>Returns <see langword="true"/> when the route may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(EvacuationRouteStatus current) => current != EvacuationRouteStatus.Retired;
}
