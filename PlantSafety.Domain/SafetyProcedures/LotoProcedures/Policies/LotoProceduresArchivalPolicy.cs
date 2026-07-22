// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.LotoProcedures
//  Layer:          Domain
//  File:           LotoProceduresArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for LotoProcedures aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.LotoProcedures.Policies;

/// <summary>Determines whether a <see cref="LotoProcedures"/> aggregate is eligible for archival.</summary>
public static class LotoProceduresArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the procedure document may be archived (retired only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.SafetyProcedures.LotoProcedures.Aggregates.LotoProcedures aggregate)
        => aggregate.Status == LotoProceduresStatus.Retired;
}
