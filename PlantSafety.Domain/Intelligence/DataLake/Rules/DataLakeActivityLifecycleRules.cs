// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Domain
//  File:           DataLakeActivityLifecycleRules.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Guards for the DataLakeActivity aggregate's lifecycle transitions. Mirrors
//                  PlantSafety.Domain.ContinuousImprovement.RCA.Rules.RCALifecycleRules.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Intelligence.DataLake.Enums;

namespace PlantSafety.Domain.Intelligence.DataLake.Rules;

/// <summary>Defines permitted lifecycle transitions for the DataLakeActivity aggregate.</summary>
public static class DataLakeActivityLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when processing may be started from <paramref name="current"/>.</summary>
    public static bool CanStartProcessing(DataLakeActivityStatus current) => current == DataLakeActivityStatus.Ingested;

    /// <summary>Returns <see langword="true"/> when the activity may be curated from <paramref name="current"/>.</summary>
    public static bool CanCurate(DataLakeActivityStatus current) => current == DataLakeActivityStatus.Processing;

    /// <summary>Returns <see langword="true"/> when the activity may be marked failed from <paramref name="current"/>.</summary>
    public static bool CanFail(DataLakeActivityStatus current) => current == DataLakeActivityStatus.Processing;

    /// <summary>Returns <see langword="true"/> when the activity may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(DataLakeActivityStatus current) =>
        current is DataLakeActivityStatus.Curated or DataLakeActivityStatus.Failed;
}
