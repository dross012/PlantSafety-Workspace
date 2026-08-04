// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.LoadSecurementChecks
//  Layer:          Domain
//  File:           LoadSecurementCheckLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the LoadSecurementCheck aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="LoadSecurementCheck"/> aggregate.</summary>
public static class LoadSecurementCheckLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when corrective action may be completed from <paramref name="current"/>.</summary>
    public static bool CanCompleteCorrectiveAction(LoadSecurementCheckStatus current) => current == LoadSecurementCheckStatus.Failed;
}
