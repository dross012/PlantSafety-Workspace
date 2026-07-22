// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.Ppe
//  Layer:          Domain
//  File:           PpeArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for Ppe aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.Ppe.Policies;

/// <summary>Determines whether a <see cref="Ppe"/> aggregate is eligible for archival.</summary>
public static class PpeArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the PPE issuance record may be archived (retired items only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.EmployeeSafety.Ppe.Aggregates.Ppe aggregate)
        => aggregate.Status == PpeStatus.Retired;
}
