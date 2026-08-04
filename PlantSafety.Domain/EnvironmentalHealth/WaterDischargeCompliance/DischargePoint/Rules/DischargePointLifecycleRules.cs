// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePoint
//  Layer:          Domain
//  File:           DischargePointLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the DischargePoint aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="DischargePoint"/> aggregate.</summary>
public static class DischargePointLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the point may be deactivated.</summary>
    public static bool CanDeactivate(DischargePointStatus current) => current == DischargePointStatus.Active;

    /// <summary>Returns <see langword="true"/> when the point may be reactivated.</summary>
    public static bool CanReactivate(DischargePointStatus current) => current == DischargePointStatus.Inactive;

    /// <summary>Returns <see langword="true"/> when the point may be decommissioned.</summary>
    public static bool CanDecommission(DischargePointStatus current) =>
        current is DischargePointStatus.Active or DischargePointStatus.Inactive;

    /// <summary>Returns <see langword="true"/> when <paramref name="status"/> is a terminal state.</summary>
    public static bool IsTerminal(DischargePointStatus status) => status == DischargePointStatus.Decommissioned;
}