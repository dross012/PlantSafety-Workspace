// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Result payload for ScheduleAuditEngagementCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>Result of a successful <see cref="ScheduleAuditEngagementCommand"/> execution.</summary>
/// <param name="AuditEngagementId">Identity of the scheduled AuditEngagement.</param>
/// <param name="Status">Lifecycle status after scheduling.</param>
/// <param name="ScheduledAtUtc">Confirmed planned execution date/time.</param>
public sealed record ScheduleAuditEngagementResult(
    Guid AuditEngagementId,
    string Status,
    DateTime ScheduledAtUtc);
