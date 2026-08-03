// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Domain
//  File:           AuditTimelineRetentionPolicy.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Governs how long ConcurrencyConflictRecord entries and reconstructed
//                  AuditTimeline entries must be retained for audit-safety purposes. Mirrors the
//                  platform's default 7-year regulatory retention window used elsewhere in
//                  Compliance. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditTimeline.Policies;

/// <summary>Governs the minimum retention window for AuditTimeline-related records.</summary>
public static class AuditTimelineRetentionPolicy
{
    /// <summary>Minimum number of years an audit timeline entry (including concurrency conflicts) must be retained.</summary>
    public const int MinimumRetentionYears = 7;

    /// <summary>Returns <see langword="true"/> when a record captured at <paramref name="occurredAtUtc"/> is still within the mandatory retention window.</summary>
    public static bool IsWithinRetentionWindow(DateTime occurredAtUtc, DateTime nowUtc)
        => occurredAtUtc >= nowUtc.AddYears(-MinimumRetentionYears);
}
