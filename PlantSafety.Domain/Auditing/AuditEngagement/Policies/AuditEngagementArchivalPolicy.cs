// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement
//  Layer:          Domain
//  File:           AuditEngagementArchivalPolicy.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for AuditEngagement aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditEngagement.Policies;

/// <summary>Determines whether a <see cref="AuditEngagement"/> aggregate is eligible for archival.</summary>
public static class AuditEngagementArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the AuditEngagement may be archived.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement aggregate)
        => aggregate.Status == AuditEngagementStatus.Closed;
}