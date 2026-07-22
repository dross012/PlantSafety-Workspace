// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MaterialHandlingStorage
//  Layer:          Domain
//  File:           MaterialHandlingStorageArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for MaterialHandlingStorage aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Policies;

/// <summary>Determines whether a <see cref="MaterialHandlingStorage"/> aggregate is eligible for archival.</summary>
public static class MaterialHandlingStorageArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the procedure document may be archived (retired only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Aggregates.MaterialHandlingStorage aggregate)
        => aggregate.Status == MaterialHandlingStorageStatus.Retired;
}
