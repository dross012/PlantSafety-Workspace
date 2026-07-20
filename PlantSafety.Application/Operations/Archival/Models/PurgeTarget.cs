// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Archival
//  Layer:          Application
//  File:           PurgeTarget.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Enumerates the 5 real, already-deployed retention purge stored
//                  procedures defined in Phase7-Governance/74-retention-archival.sql and
//                  run daily (Phase 18 Task Scheduler, Invoke-PlantSafetyPurge.ps1). One
//                  member per procedure. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Operations.Archival.Models;

/// <summary>One of the 5 live retention/purge stored procedures.</summary>
public enum PurgeTarget
{
    /// <summary>PlantSafety.AuditDB.dbo.sp_AuditLog_Purge (dbo.AuditLog).</summary>
    AuditLog = 0,

    /// <summary>
    /// PlantSafety.ErrorDB.dbo.sp_ErrorLog_Purge (dbo.ErrorLog). This proc is expected to
    /// log a "Blocked" ArchivalJob outcome -- the target table has an immutability trigger
    /// (TR_ErrorLog_NoDelete) that unconditionally blocks all deletes. This is a designed,
    /// logged outcome, not a failure.
    /// </summary>
    ErrorLog = 1,

    /// <summary>PlantSafety.IntegrationDB.Integration.sp_IntegrationLogEntry_Purge (Integration.IntegrationLogEntry).</summary>
    IntegrationLogEntry = 2,

    /// <summary>PlantSafety.IntegrationDB.Integration.sp_ReplayRecord_Purge (Integration.ReplayRecord).</summary>
    ReplayRecord = 3,

    /// <summary>PlantSafety.DiagnosticsDB.Diagnostics.sp_DiagnosticEvent_Purge (Diagnostics.DiagnosticEvent).</summary>
    DiagnosticEvent = 4
}
