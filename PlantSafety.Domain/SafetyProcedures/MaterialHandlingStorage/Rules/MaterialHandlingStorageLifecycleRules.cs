// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MaterialHandlingStorage
//  Layer:          Domain
//  File:           MaterialHandlingStorageLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the MaterialHandlingStorage aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="MaterialHandlingStorage"/> aggregate.</summary>
public static class MaterialHandlingStorageLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the procedure may be submitted for approval from <paramref name="current"/>.</summary>
    public static bool CanSubmitForApproval(MaterialHandlingStorageStatus current) => current == MaterialHandlingStorageStatus.Draft;

    /// <summary>Returns <see langword="true"/> when the procedure may be published from <paramref name="current"/>.</summary>
    public static bool CanPublish(MaterialHandlingStorageStatus current) => current == MaterialHandlingStorageStatus.PendingApproval;

    /// <summary>Returns <see langword="true"/> when an obstruction may be flagged from <paramref name="current"/>.</summary>
    public static bool CanFlagObstruction(MaterialHandlingStorageStatus current) => current == MaterialHandlingStorageStatus.Published;

    /// <summary>Returns <see langword="true"/> when an obstruction may be resolved from <paramref name="current"/>.</summary>
    public static bool CanResolveObstruction(MaterialHandlingStorageStatus current) => current == MaterialHandlingStorageStatus.ObstructionFlagged;
}
