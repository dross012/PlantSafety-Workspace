// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.ActiveMonitoringAuditing
//  Layer:          Domain
//  File:           ActiveMonitoringAuditingStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real observation lifecycle states for the ActiveMonitoringAuditing aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Enums;

/// <summary>Lifecycle states for the <see cref="ActiveMonitoringAuditing"/> aggregate root.</summary>
public enum ActiveMonitoringAuditingStatus
{
    /// <summary>A monitoring/audit visit has been scheduled.</summary>
    Scheduled = 0,

    /// <summary>Observation recorded; no unsafe conditions found.</summary>
    ObservationRecorded = 1,

    /// <summary>An unsafe condition or behavior was flagged during the observation.</summary>
    UnsafeConditionFlagged = 2,

    /// <summary>A follow-up visit has been scheduled to verify correction.</summary>
    FollowUpScheduled = 3,

    /// <summary>Flagged condition verified resolved.</summary>
    Resolved = 4,
}
