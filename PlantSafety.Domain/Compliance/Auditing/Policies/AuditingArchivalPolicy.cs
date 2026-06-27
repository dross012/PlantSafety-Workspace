// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Domain
//  File:           AuditingArchivalPolicy.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for Auditing aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Auditing.Policies;

/// <summary>Determines whether a <see cref="Auditing"/> aggregate is eligible for archival.</summary>
public static class AuditingArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the Auditing may be archived.</summary>
    public static bool IsEligibleForArchival(Auditing aggregate)
        => aggregate.Status == AuditingStatus.Closed;
}
