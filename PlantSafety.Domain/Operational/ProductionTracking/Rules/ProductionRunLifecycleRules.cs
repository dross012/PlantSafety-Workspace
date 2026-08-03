// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Domain
//  File:           ProductionRunLifecycleRules.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Guards for the ProductionRun aggregate's lifecycle transitions.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.ProductionTracking.Enums;

namespace PlantSafety.Domain.Operational.ProductionTracking.Rules;

/// <summary>Defines permitted lifecycle transitions for the ProductionRun aggregate.</summary>
public static class ProductionRunLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the run may be paused from <paramref name="current"/>.</summary>
    public static bool CanPause(ProductionRunStatus current) => current == ProductionRunStatus.Running;

    /// <summary>Returns <see langword="true"/> when the run may be resumed from <paramref name="current"/>.</summary>
    public static bool CanResume(ProductionRunStatus current) => current == ProductionRunStatus.Paused;

    /// <summary>Returns <see langword="true"/> when a timeline event may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordEvent(ProductionRunStatus current) =>
        current is ProductionRunStatus.Running or ProductionRunStatus.Paused;

    /// <summary>Returns <see langword="true"/> when a batch may be added from <paramref name="current"/>.</summary>
    public static bool CanAddBatch(ProductionRunStatus current) =>
        current is ProductionRunStatus.Running or ProductionRunStatus.Paused;

    /// <summary>Returns <see langword="true"/> when the run may be ended (Completed/Failed) from <paramref name="current"/>.</summary>
    public static bool CanEnd(ProductionRunStatus current) =>
        current is ProductionRunStatus.Running or ProductionRunStatus.Paused;
}
