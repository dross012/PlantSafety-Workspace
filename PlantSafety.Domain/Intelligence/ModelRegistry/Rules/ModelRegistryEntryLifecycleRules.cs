// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Domain
//  File:           ModelRegistryEntryLifecycleRules.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    SAFETY-CRITICAL guards for the ModelRegistryEntry aggregate's lifecycle
//                  transitions, including the Safety Rule 4.5 deployment-eligibility gate ported
//                  from the orphaned, dead
//                  Intelligence.Domain/ModelRegistry/ValueObjects/ModelStatus.cs Smart Enum's
//                  IsTerminal/IsDeployable instance properties. Mirrors
//                  PlantSafety.Domain.Intelligence.DataLake.Rules.DataLakeActivityLifecycleRules.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Intelligence.ModelRegistry.Enums;

namespace PlantSafety.Domain.Intelligence.ModelRegistry.Rules;

/// <summary>Defines permitted lifecycle transitions and deployment eligibility for the ModelRegistryEntry aggregate.</summary>
public static class ModelRegistryEntryLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the entry may be submitted for approval from <paramref name="current"/>.</summary>
    public static bool CanSubmitForApproval(ModelRegistryEntryStatus current) => current == ModelRegistryEntryStatus.Draft;

    /// <summary>Returns <see langword="true"/> when the entry may be approved from <paramref name="current"/>.</summary>
    public static bool CanApprove(ModelRegistryEntryStatus current) => current == ModelRegistryEntryStatus.PendingApproval;

    /// <summary>Returns <see langword="true"/> when the entry may be deprecated from <paramref name="current"/>.</summary>
    public static bool CanDeprecate(ModelRegistryEntryStatus current) => current == ModelRegistryEntryStatus.Approved;

    /// <summary>Returns <see langword="true"/> when the entry may be retired from <paramref name="current"/>. Retired is terminal.</summary>
    public static bool CanRetire(ModelRegistryEntryStatus current) => !IsTerminal(current);

    /// <summary>Returns <see langword="true"/> when <paramref name="status"/> is a terminal state (no further transitions).</summary>
    public static bool IsTerminal(ModelRegistryEntryStatus status) => status == ModelRegistryEntryStatus.Retired;

    /// <summary>
    /// SAFETY-CRITICAL (Safety Rule 4.5): Returns <see langword="true"/> only when
    /// <paramref name="status"/> is <see cref="ModelRegistryEntryStatus.Approved"/>. Fail safe --
    /// every other status, including any future addition, is not deployable by default.
    /// </summary>
    public static bool IsDeployable(ModelRegistryEntryStatus status) => status == ModelRegistryEntryStatus.Approved;
}
