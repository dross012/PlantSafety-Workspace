// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Domain
//  File:           WorkerWellBeingLifecycleRules.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the WorkerWellBeing aggregate.
//                  Phase 4 full build-out. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.WorkerWellBeing.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="WorkerWellBeing"/> aggregate.</summary>
public static class WorkerWellBeingLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(WorkerWellBeingStatus current) => current == WorkerWellBeingStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(WorkerWellBeingStatus current) => current == WorkerWellBeingStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(WorkerWellBeingStatus current) => current == WorkerWellBeingStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(WorkerWellBeingStatus current) => current == WorkerWellBeingStatus.Closed;
}
