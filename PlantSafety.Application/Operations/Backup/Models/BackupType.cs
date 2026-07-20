// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Backup
//  Layer:          Application
//  File:           BackupType.cs
//  Created:        2026-07-19
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Backup type selector mirroring the msdb.dbo.usp_PlantSafety_Backup<Type>
//                  stored procedures (Phase 12, 120-backup-jobs-full-diff-log.sql) invoked
//                  by Invoke-PlantSafetyBackup.ps1 (Phase 18). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Operations.Backup.Models;

/// <summary>
/// Selects which SQL Server backup type to trigger. Values map 1:1 onto the
/// <c>-BackupType</c> parameter of <c>Invoke-PlantSafetyBackup.ps1</c> and the
/// <c>msdb.dbo.usp_PlantSafety_Backup&lt;Type&gt;</c> stored procedures.
/// </summary>
public enum BackupType
{
    /// <summary>Full backup — checksummed, uncompressed (Express edition). Weekly cadence.</summary>
    Full = 0,

    /// <summary>Differential backup — requires a prior FULL backup. Daily cadence.</summary>
    Diff = 1,

    /// <summary>Transaction log backup — only applies to databases in FULL/BULK_LOGGED recovery model.</summary>
    Log = 2
}
