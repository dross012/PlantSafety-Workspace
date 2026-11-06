// ============================================================================
//  Project:        Shared.AuditLogging
//  Module:         Abstractions.Models
//  Layer:          Abstractions
//  File:           AuditLogEntry.cs
//  Created:        2026-07-26
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Projection of a single row from [PlantSafety.AuditDB].dbo.AuditLog, the
//                  shared, cross-database, append-only platform audit trail written to by
//                  hundreds of stored procedures across all 8 module databases via the
//                  per-database dbo.AuditLog synonym (see PlantSafety.Infrastructure's
//                  Persistence\Migrations\Phase8-Structural\83-synonyms-audit-error.sql,
//                  the table's origin). Columns below are the confirmed, evidence-backed
//                  column list, built from live INSERT statements: TableName,
//                  SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed,
//                  ErrorContext, Severity, Module (PlantSafety.Infrastructure's
//                  Phase17-Reliability\170-fix-errorlog-masking-bug.sql, line ~69), plus
//                  BeforeSnapshot/AfterSnapshot (same file, CATCH-block audit insert at
//                  line ~111, and repeated identically across Phase5-Views\50-coredb-
//                  views.sql and every Phase6-StoredProcedures *-procedures.sql catch
//                  block). No identity/primary-key column name is evidenced anywhere in
//                  the migration scripts scanned, so none is invented here -- see
//                  IAuditLogRepository remarks.
//
//                  DIFF VIEW SCOPE NOTE: BeforeSnapshot/AfterSnapshot are real columns,
//                  but every currently-live INSERT in the migration scripts passes them
//                  as NULL, NULL -- no stored procedure in this codebase populates them
//                  today. The only column that reliably carries change detail today is
//                  the free-text ErrorContext field (typically "Key=Value; Key=Value"
//                  pairs, e.g. "AiGovernanceId=...; CreatedBy=...; UpdatedBy=..."). This
//                  record surfaces BeforeSnapshot/AfterSnapshot as-is (usually null today)
//                  AND exposes ErrorContext for callers to parse into a readable "Change
//                  Detail" panel -- NOT a true structured before/after diff.
//
//                  PROVENANCE: This type originated as PlantSafety.Application's
//                  Operations.AuditViewer.Models.AuditLogEntryDto (created 2026-07-20) and
//                  was extracted here 2026-07-26 so both PlantSafety (Audit Viewer UI) and
//                  Reporting (Audit Report PDF) can consume it, since dbo.AuditLog is
//                  server-level platform data, not PlantSafety-tenant data. Renamed
//                  AuditLogEntryDto -> AuditLogEntry (no other .Abstractions-style shared
//                  contract in this solution suffixes cross-module records with "Dto").
//                  Additive-only evolution.
// ============================================================================
namespace Shared.AuditLogging.Abstractions.Models;

/// <summary>
/// Projection of a single row from [PlantSafety.AuditDB].dbo.AuditLog, the shared,
/// cross-database, append-only platform audit trail.
/// </summary>
/// <param name="TableName">The domain table/entity the audited operation targeted (e.g. "AiGovernance").</param>
/// <param name="SourceDatabase">The database the write originated from (DB_NAME() at insert time).</param>
/// <param name="Operation">The audited operation ("INSERT", "UPDATE", "DELETE", "VIEW", ...).</param>
/// <param name="ChangedBy">The SQL Server principal (SYSTEM_USER) that performed the operation.</param>
/// <param name="ChangeTime">UTC timestamp the audit row was written (SYSUTCDATETIME()).</param>
/// <param name="ViewerAccessed">True when the audited operation was a read/view rather than a write.</param>
/// <param name="ErrorContext">
/// Free-text detail string, typically "Key=Value; Key=Value" parameter pairs. This is the ONLY
/// field in the confirmed schema that carries change-level detail today -- see the diff-view
/// scope note on this file's header comment.
/// </param>
/// <param name="Severity">Numeric severity (0 for best-effort success-path audit rows; ERROR_SEVERITY() on failure-path rows).</param>
/// <param name="Module">The stored procedure or module that produced the audit row (OBJECT_NAME(@@PROCID)).</param>
/// <param name="BeforeSnapshot">Reserved structured "before" snapshot column. Always null in every current write path.</param>
/// <param name="AfterSnapshot">Reserved structured "after" snapshot column. Always null in every current write path.</param>
public sealed record AuditLogEntry(
    string TableName,
    string SourceDatabase,
    string Operation,
    string ChangedBy,
    DateTime ChangeTime,
    bool ViewerAccessed,
    string? ErrorContext,
    int Severity,
    string Module,
    string? BeforeSnapshot,
    string? AfterSnapshot);
