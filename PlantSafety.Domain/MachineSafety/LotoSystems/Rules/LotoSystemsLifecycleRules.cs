// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.LotoSystems
//  Layer:          Domain
//  File:           LotoSystemsLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the LotoSystems aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.LotoSystems.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="LotoSystems"/> aggregate.</summary>
public static class LotoSystemsLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when isolation may be applied from <paramref name="current"/>.</summary>
    public static bool CanApplyIsolation(LotoSystemsStatus current) =>
        current is LotoSystemsStatus.Documented or LotoSystemsStatus.Released;

    /// <summary>Returns <see langword="true"/> when zero-energy may be verified from <paramref name="current"/>.</summary>
    public static bool CanVerifyZeroEnergy(LotoSystemsStatus current) => current == LotoSystemsStatus.IsolationApplied;

    /// <summary>Returns <see langword="true"/> when isolation may be released from <paramref name="current"/>.</summary>
    public static bool CanRelease(LotoSystemsStatus current) => current == LotoSystemsStatus.ZeroEnergyVerified;
}
