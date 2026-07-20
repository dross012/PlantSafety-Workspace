// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Backup
//  Layer:          Application
//  File:           BackupTriggerResultDto.cs
//  Created:        2026-07-19
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Result of an on-demand backup trigger. The underlying PowerShell
//                  process (Invoke-PlantSafetyBackup.ps1) is launched non-blocking;
//                  this DTO reflects the "Started" acknowledgement only, not the
//                  eventual success/failure of the backup itself (query
//                  GetBackupHistoryQuery / GetBackupHealthQuery afterwards for that).
//                  Additive-only evolution.
// ============================================================================
using System;

namespace PlantSafety.Application.Operations.Backup.Models;

/// <summary>Acknowledgement returned immediately after a backup process is launched.</summary>
/// <param name="BackupType">The backup type that was triggered.</param>
/// <param name="ProcessId">The OS process id of the launched <c>powershell.exe</c> host, if available.</param>
/// <param name="StartedUtc">UTC timestamp the trigger request was accepted.</param>
/// <param name="Status">Always <c>"Started"</c> on success; handlers surface failures via <see cref="PlantSafety.Application.Common.Results.Result{T}"/>.</param>
/// <param name="LogDirectory">Directory the backup task log is written to (see Invoke-PlantSafetyBackup.ps1).</param>
public sealed record BackupTriggerResultDto(
    BackupType BackupType,
    int? ProcessId,
    DateTime StartedUtc,
    string Status,
    string LogDirectory);
