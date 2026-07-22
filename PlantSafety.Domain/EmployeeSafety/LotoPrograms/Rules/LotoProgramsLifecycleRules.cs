// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.LotoPrograms
//  Layer:          Domain
//  File:           LotoProgramsLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the LotoPrograms aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.LotoPrograms.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="LotoPrograms"/> aggregate.</summary>
public static class LotoProgramsLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when isolation may be applied from <paramref name="current"/>.</summary>
    public static bool CanApplyIsolation(LotoProgramsStatus current) => current == LotoProgramsStatus.Documented;

    /// <summary>Returns <see langword="true"/> when zero-energy may be verified from <paramref name="current"/>.</summary>
    public static bool CanVerifyZeroEnergy(LotoProgramsStatus current) => current == LotoProgramsStatus.IsolationApplied;

    /// <summary>Returns <see langword="true"/> when the isolation may be released from <paramref name="current"/>.</summary>
    public static bool CanRelease(LotoProgramsStatus current) => current == LotoProgramsStatus.ZeroEnergyVerified;

    /// <summary>Returns <see langword="true"/> when an annual audit may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordAnnualAudit(LotoProgramsStatus current) => current != LotoProgramsStatus.Documented;
}
