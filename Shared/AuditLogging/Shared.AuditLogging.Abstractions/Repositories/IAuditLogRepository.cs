// ============================================================================
//  Project:        Shared.AuditLogging
//  Module:         Abstractions.Repositories
//  Layer:          Abstractions
//  File:           IAuditLogRepository.cs
//  Created:        2026-07-26
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction over the shared, cross-database
//                  [PlantSafety.AuditDB].dbo.AuditLog table -- real platform audit
//                  infrastructure written to by hundreds of stored procedures across
//                  all 8 module databases (see PlantSafety.Infrastructure's Persistence\
//                  Migrations\Phase8-Structural\83-synonyms-audit-error.sql). Implemented
//                  in Shared.AuditLogging.Infrastructure against PlantSafety.AuditDB
//                  directly -- NOT tenant-scoped, server-level platform data.
//
//                  This is the SINGLE canonical read path for dbo.AuditLog. It was
//                  originally implemented inside PlantSafety (PlantSafety.Application's
//                  Operations.AuditViewer.Repositories.IAuditLogRepository, created
//                  2026-07-20) and extracted into this shared module on 2026-07-26 so
//                  that PlantSafety's Audit Viewer UI and Reporting's Audit Report PDF
//                  both consume the exact same repository rather than each module
//                  reimplementing (or, worse, one module taking a forbidden cross-module
//                  dependency on the other) -- see this module's docs\README.md.
//
//                  SAFETY: This repository is deliberately READ-ONLY. AuditLog is an
//                  append-only audit trail; no method here may INSERT, UPDATE, or
//                  DELETE against it.
//
//                  No identity/primary-key column name is evidenced anywhere in the
//                  scanned migration scripts for AuditLog, so results are not exposed
//                  with a synthetic row id -- callers page/order by ChangeTime.
//                  Additive-only evolution.
// ============================================================================
using Shared.AuditLogging.Abstractions.Models;

namespace Shared.AuditLogging.Abstractions.Repositories;

/// <summary>Read-only repository over the shared [PlantSafety.AuditDB].dbo.AuditLog table.</summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Searches AuditLog by the given optional filters, newest first, with pagination.
    /// Never mutates AuditLog -- SELECT only (via the dbo.usp_SearchAuditLog stored procedure).
    /// </summary>
    Task<AuditLogSearchResult> SearchAsync(AuditLogSearchCriteria criteria, CancellationToken cancellationToken);
}
