// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityMeetingRecord
//  Layer:          Domain
//  File:           CommunityMeetingRecordStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the CommunityMeetingRecord aggregate root (Outreach &amp;
//                  Notification History subdomain -- scheduled community meeting cadence). This
//                  aggregate records that a community meeting was held (a completed-events log,
//                  not a scheduling calendar); the periodic scheduling cadence itself is owned by
//                  a Scheduling-module background job that raises RecordCommunityMeeting commands
//                  on cadence. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CommunitySafety.CommunityMeetingRecord.Enums;

/// <summary>Lifecycle states for the <see cref="Aggregates.CommunityMeetingRecord"/> aggregate root.</summary>
public enum CommunityMeetingRecordStatus
{
    /// <summary>Meeting has been recorded. Initial state on Record.</summary>
    Recorded = 0,

    /// <summary>Meeting record has been archived for long-term retention. Terminal state.</summary>
    Archived = 1,
}
