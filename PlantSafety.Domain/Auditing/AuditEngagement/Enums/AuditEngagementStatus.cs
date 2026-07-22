// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement
//  Layer:          Domain
//  File:           AuditEngagementStatus.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the AuditEngagement aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditEngagement.Enums;

/// <summary>Lifecycle states for the <see cref="AuditEngagement"/> aggregate root.</summary>
public enum AuditEngagementStatus
{
    /// <summary>Record has been created and awaits processing.</summary>
    Created = 0,

    /// <summary>Record is active and under management.</summary>
    Active = 1,

    /// <summary>Record has been completed or resolved.</summary>
    Completed = 2,

    /// <summary>Record has been closed with no further action required.</summary>
    Closed = 3,

    /// <summary>Record has been archived for long-term retention.</summary>
    Archived = 4,

    /// <summary>Engagement has been planned and scheduled (lead auditor assigned, date set) but not yet started. Additive value appended per Handbook Rule 2.7 (enum additive-only evolution).</summary>
    Scheduled = 5,
}