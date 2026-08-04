// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePermit
//  Layer:          Domain
//  File:           DischargePermitLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the DischargePermit aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="DischargePermit"/> aggregate.</summary>
public static class DischargePermitLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when renewal is permitted from <paramref name="current"/>.</summary>
    public static bool CanRenew(DischargePermitStatus current) =>
        current is DischargePermitStatus.Active or DischargePermitStatus.ExpiringSoon or DischargePermitStatus.Expired;

    /// <summary>Returns <see langword="true"/> when the permit may be flagged as expiring soon.</summary>
    public static bool CanFlagExpiringSoon(DischargePermitStatus current) => current == DischargePermitStatus.Active;

    /// <summary>Returns <see langword="true"/> when the permit may be suspended.</summary>
    public static bool CanSuspend(DischargePermitStatus current) =>
        current is DischargePermitStatus.Active or DischargePermitStatus.ExpiringSoon;

    /// <summary>Returns <see langword="true"/> when the permit may be revoked.</summary>
    public static bool CanRevoke(DischargePermitStatus current) =>
        current is DischargePermitStatus.Active or DischargePermitStatus.ExpiringSoon or DischargePermitStatus.Suspended;

    /// <summary>Returns <see langword="true"/> when <paramref name="status"/> is a terminal state.</summary>
    public static bool IsTerminal(DischargePermitStatus status) =>
        status is DischargePermitStatus.Expired or DischargePermitStatus.Revoked;
}