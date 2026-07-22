// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.ActiveMonitoringAuditing
//  Layer:          Domain
//  File:           ActiveMonitoringAuditingLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the ActiveMonitoringAuditing aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="ActiveMonitoringAuditing"/> aggregate.</summary>
public static class ActiveMonitoringAuditingLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when an observation may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordObservation(ActiveMonitoringAuditingStatus current) =>
        current is ActiveMonitoringAuditingStatus.Scheduled or ActiveMonitoringAuditingStatus.FollowUpScheduled;

    /// <summary>Returns <see langword="true"/> when a follow-up may be scheduled from <paramref name="current"/>.</summary>
    public static bool CanScheduleFollowUp(ActiveMonitoringAuditingStatus current) => current == ActiveMonitoringAuditingStatus.UnsafeConditionFlagged;

    /// <summary>Returns <see langword="true"/> when the observation may be resolved from <paramref name="current"/>.</summary>
    public static bool CanResolve(ActiveMonitoringAuditingStatus current) => current == ActiveMonitoringAuditingStatus.FollowUpScheduled;
}
