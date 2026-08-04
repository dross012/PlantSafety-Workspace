// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.LoadSecurementChecks
//  Layer:          Domain
//  File:           LoadSecurementCheckArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for LoadSecurementCheck aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Policies;

/// <summary>Determines whether a <see cref="LoadSecurementCheck"/> aggregate is eligible for archival.</summary>
public static class LoadSecurementCheckArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the check record may be archived (passed or corrective-action-completed only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Aggregates.LoadSecurementCheck aggregate)
        => aggregate.Status is LoadSecurementCheckStatus.Passed or LoadSecurementCheckStatus.CorrectiveActionCompleted;
}
