// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditFinding
//  Layer:          Domain
//  File:           AuditFindingArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for AuditFinding aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditFinding.Policies;

/// <summary>Determines whether a <see cref="AuditFinding"/> aggregate is eligible for archival.</summary>
public static class AuditFindingArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the AuditFinding may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Auditing.AuditFinding.Aggregates.AuditFinding aggregate)
        => aggregate.Status == AuditFindingStatus.Closed;
}