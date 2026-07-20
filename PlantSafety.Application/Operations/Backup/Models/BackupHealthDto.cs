// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Backup
//  Layer:          Application
//  File:           BackupHealthDto.cs
//  Created:        2026-07-19
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Derived backup-health status for a single PlantSafety.* database.
//                  Health is computed from the recency of the most recent successful
//                  FULL backup found in msdb.dbo.backupset:
//                    Green  — last FULL backup within BackupOptions.HealthWarnThresholdHours (default 24h).
//                    Yellow — last FULL backup older than the warn threshold but within
//                             BackupOptions.HealthCriticalThresholdHours (default 48h).
//                    Red    — last FULL backup older than the critical threshold, or no
//                             FULL backup has ever completed for this database.
//                  Thresholds are configuration-driven (Operations:Backup section) so
//                  operators can tune them without a code change. Additive-only evolution.
// ============================================================================
using System;

namespace PlantSafety.Application.Operations.Backup.Models;

/// <summary>Derived backup-health status for a single database.</summary>
/// <param name="DatabaseName">The PlantSafety.* database.</param>
/// <param name="LastFullBackupUtc">Timestamp of the most recent successful FULL backup, or null if none exists.</param>
/// <param name="HealthStatus">"Green" | "Yellow" | "Red" — see remarks on <see cref="BackupHealthDto"/> above.</param>
/// <param name="Reason">Human-readable explanation of the computed status.</param>
public sealed record BackupHealthDto(
    string DatabaseName,
    DateTime? LastFullBackupUtc,
    string HealthStatus,
    string Reason);
