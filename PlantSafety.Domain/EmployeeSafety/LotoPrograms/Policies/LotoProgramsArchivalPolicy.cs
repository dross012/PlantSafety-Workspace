// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.LotoPrograms
//  Layer:          Domain
//  File:           LotoProgramsArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for LotoPrograms aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.LotoPrograms.Policies;

/// <summary>Determines whether a <see cref="LotoPrograms"/> aggregate is eligible for archival.</summary>
public static class LotoProgramsArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the LOTO program may be archived (released and not flagged non-compliant).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.EmployeeSafety.LotoPrograms.Aggregates.LotoPrograms aggregate)
        => aggregate.Status == LotoProgramsStatus.Released;
}
