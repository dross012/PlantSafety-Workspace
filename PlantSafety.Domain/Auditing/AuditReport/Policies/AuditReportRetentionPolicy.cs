// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Domain
//  File:           AuditReportRetentionPolicy.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Governs how long generated AuditReport artifacts must be retained.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditReport.Policies;

/// <summary>Governs the minimum retention window for generated AuditReport artifacts.</summary>
public static class AuditReportRetentionPolicy
{
    /// <summary>Minimum number of years a generated audit report must be retained.</summary>
    public const int MinimumRetentionYears = 7;

    /// <summary>Returns <see langword="true"/> when a report generated at <paramref name="generatedAtUtc"/> is still within the mandatory retention window.</summary>
    public static bool IsWithinRetentionWindow(DateTime generatedAtUtc, DateTime nowUtc)
        => generatedAtUtc >= nowUtc.AddYears(-MinimumRetentionYears);
}
