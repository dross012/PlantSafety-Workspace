// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessEvents
//  Layer:          Domain
//  File:           AccessEventLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted processing-status transitions for the AccessEvent
//                  aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Rules;

/// <summary>Defines permitted processing-status transitions for the <see cref="AccessEvent"/> aggregate.</summary>
public static class AccessEventLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the event may be marked processed from <paramref name="current"/>.</summary>
    public static bool CanMarkProcessed(AccessEventStatus current) => current == AccessEventStatus.Ingested;

    /// <summary>Returns <see langword="true"/> when the event may be flagged anomalous from <paramref name="current"/>.</summary>
    public static bool CanFlagAnomaly(AccessEventStatus current) =>
        current is AccessEventStatus.Ingested or AccessEventStatus.Processed;
}
