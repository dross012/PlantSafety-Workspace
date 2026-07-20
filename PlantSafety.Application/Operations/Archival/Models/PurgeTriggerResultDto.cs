// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Archival
//  Layer:          Application
//  File:           PurgeTriggerResultDto.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Real, synchronous outcome of triggering one retention purge stored
//                  procedure. Unlike Operations.Backup's fire-and-forget "Started"
//                  acknowledgement, this reflects the ACTUAL Completed/Blocked/Failed
//                  status the procedure itself logged to Archival.ArchivalJob, because the
//                  purge procedures are fast, bounded DELETE statements invoked directly
//                  via EXEC rather than a long-running external process. Additive-only
//                  evolution.
// ============================================================================
using System;

namespace PlantSafety.Application.Operations.Archival.Models;

/// <summary>Real outcome of triggering a single retention purge stored procedure.</summary>
/// <param name="Target">The purge target that was triggered.</param>
/// <param name="Zone">The Archival.TierPolicy.Zone this run applies to.</param>
/// <param name="Status">"Completed" | "Blocked" | "Failed" | "Unknown" (no ArchivalJob row was found after execution).</param>
/// <param name="ArchivalJobId">The Archival.ArchivalJob row id logged by the procedure, if found.</param>
/// <param name="StartedUtc">When the procedure run started, as logged by the procedure itself.</param>
/// <param name="CompletedUtc">When the procedure run completed, as logged by the procedure itself.</param>
/// <param name="Detail">Human-readable outcome detail (row count purged, block reason, or failure message).</param>
public sealed record PurgeTriggerResultDto(
    PurgeTarget Target,
    string Zone,
    string Status,
    Guid? ArchivalJobId,
    DateTime? StartedUtc,
    DateTime? CompletedUtc,
    string? Detail);
