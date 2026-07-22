// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.ErgonomicsAndMaterialHandling
//  Layer:          Domain
//  File:           ErgonomicsAndMaterialHandlingArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for ErgonomicsAndMaterialHandling aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Policies;

/// <summary>Determines whether a <see cref="ErgonomicsAndMaterialHandling"/> aggregate is eligible for archival.</summary>
public static class ErgonomicsAndMaterialHandlingArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the assessment may be archived (closed only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Aggregates.ErgonomicsAndMaterialHandling aggregate)
        => aggregate.Status == ErgonomicsAndMaterialHandlingStatus.Closed;
}
