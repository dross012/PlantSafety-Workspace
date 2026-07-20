// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Backup
//  Layer:          Application
//  File:           DatabaseSizeSnapshotDto.cs
//  Created:        2026-07-19
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    A single row from msdb.dbo.PlantSafety_DatabaseSizeLog (Phase 18,
//                  180-database-size-log.sql). Additive-only evolution.
// ============================================================================
using System;

namespace PlantSafety.Application.Operations.Backup.Models;

/// <summary>A single file-level database size snapshot captured by usp_PlantSafety_LogDatabaseSizes.</summary>
/// <param name="CapturedUtc">When the snapshot was taken.</param>
/// <param name="DatabaseName">The PlantSafety.* database.</param>
/// <param name="FileLogicalName">The data/log file's logical name.</param>
/// <param name="FileType">"ROWS" (data file) or "LOG" (transaction log file).</param>
/// <param name="SizeMB">Allocated file size in megabytes.</param>
/// <param name="UsedSpaceMB">Actual used space in megabytes, or null if the FILEPROPERTY lookup failed.</param>
public sealed record DatabaseSizeSnapshotDto(
    DateTime CapturedUtc,
    string DatabaseName,
    string FileLogicalName,
    string FileType,
    decimal SizeMB,
    decimal? UsedSpaceMB);
