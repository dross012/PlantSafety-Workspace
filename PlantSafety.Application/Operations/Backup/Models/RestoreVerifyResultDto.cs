// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Backup
//  Layer:          Application
//  File:           RestoreVerifyResultDto.cs
//  Created:        2026-07-19
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Result of msdb.dbo.usp_PlantSafety_RestoreTest (Phase 12,
//                  123-restore-test.sql) — a SAFE, non-destructive restore-verify
//                  that restores the latest FULL backup into an isolated
//                  &lt;DatabaseName&gt;_DRTest database, runs DBCC CHECKDB, then always
//                  drops the test database. Never touches the live database. This is
//                  NOT a production restore — see the DR-manual-restore-runbook.md for
//                  that deliberately manual, human-run procedure. Additive-only evolution.
// ============================================================================
using System;

namespace PlantSafety.Application.Operations.Backup.Models;

/// <summary>Row read back from dbo.PlantSafety_MaintenanceLog for a RESTORE_TEST run.</summary>
/// <param name="MaintenanceLogId">Primary key of the logged run.</param>
/// <param name="DatabaseName">The database whose latest FULL backup was restore-tested.</param>
/// <param name="StartUtc">When the restore-test started.</param>
/// <param name="EndUtc">When the restore-test finished, or null if still in flight.</param>
/// <param name="Result">"OK" | "FAILED".</param>
/// <param name="Details">Human-readable detail (restored file, CHECKDB outcome, or the error message).</param>
public sealed record RestoreVerifyResultDto(
    long MaintenanceLogId,
    string DatabaseName,
    DateTime StartUtc,
    DateTime? EndUtc,
    string Result,
    string? Details);
