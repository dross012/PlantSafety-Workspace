// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Backup
//  Layer:          Application
//  File:           BackupHistoryItemDto.cs
//  Created:        2026-07-19
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    A single row from msdb.dbo.backupset / backupmediafamily — the
//                  standard SQL Server backup history catalog. Additive-only evolution.
// ============================================================================
using System;

namespace PlantSafety.Application.Operations.Backup.Models;

/// <summary>Projection of a single completed backup, sourced from msdb.dbo.backupset.</summary>
/// <param name="DatabaseName">The PlantSafety.* database the backup was taken against.</param>
/// <param name="BackupTypeDescription">Human-readable backup type ("Full", "Differential", "Transaction Log").</param>
/// <param name="BackupFinishDateUtc">When the backup completed.</param>
/// <param name="SizeMB">Backup set size in megabytes.</param>
/// <param name="PhysicalDeviceName">Full path to the .bak/.trn file on disk.</param>
public sealed record BackupHistoryItemDto(
    string DatabaseName,
    string BackupTypeDescription,
    DateTime BackupFinishDateUtc,
    decimal SizeMB,
    string PhysicalDeviceName);
