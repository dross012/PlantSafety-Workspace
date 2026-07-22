// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.ErgonomicsWorkstationDesign
//  Layer:          Domain
//  File:           ErgonomicsWorkstationDesignArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for ErgonomicsWorkstationDesign
//                  aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Policies;

/// <summary>Determines whether a <see cref="ErgonomicsWorkstationDesign"/> aggregate is eligible for archival.</summary>
public static class ErgonomicsWorkstationDesignArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the procedure document may be archived (retired only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Aggregates.ErgonomicsWorkstationDesign aggregate)
        => aggregate.Status == ErgonomicsWorkstationDesignStatus.Retired;
}
