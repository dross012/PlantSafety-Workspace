// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.ActiveMonitoringAuditing
//  Layer:          Domain
//  File:           ActiveMonitoringAuditingArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for ActiveMonitoringAuditing aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Policies;

/// <summary>Determines whether a <see cref="ActiveMonitoringAuditing"/> aggregate is eligible for archival.</summary>
public static class ActiveMonitoringAuditingArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the observation record may be archived (recorded clean or resolved).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Aggregates.ActiveMonitoringAuditing aggregate)
        => aggregate.Status is ActiveMonitoringAuditingStatus.ObservationRecorded or ActiveMonitoringAuditingStatus.Resolved;
}
