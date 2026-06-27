// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Domain
//  File:           ProceduresArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for Procedures aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Procedures.Policies;

/// <summary>Determines whether a <see cref="Procedures"/> aggregate is eligible for archival.</summary>
public static class ProceduresArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the Procedures may be archived.</summary>
    public static bool IsEligibleForArchival(Procedures aggregate)
        => aggregate.Status == ProceduresStatus.Closed;
}
