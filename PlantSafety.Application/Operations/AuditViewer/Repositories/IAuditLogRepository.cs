// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.AuditViewer
//  Layer:          Application
//  File:           IAuditLogRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction over the shared, cross-database
//                  [PlantSafety.AuditDB].dbo.AuditLog table -- real platform audit
//                  infrastructure written to by hundreds of stored procedures across
//                  all 8 module databases (see Phase8-Structural\83-synonyms-audit-
//                  error.sql). Implemented in the Infrastructure layer against
//                  PlantSafety.AuditDB directly -- NOT tenant-scoped, server-level
//                  platform data, matching the Operations.Backup/SchemaGuard pattern
//                  of opening a dedicated connection rather than going through
//                  ISqlConnectionFactory.
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
using System.Threading;
using System.Threading.Tasks;

using PlantSafety.Application.Operations.AuditViewer.Models;

namespace PlantSafety.Application.Operations.AuditViewer.Repositories;

/// <summary>Read-only repository over the shared [PlantSafety.AuditDB].dbo.AuditLog table.</summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Searches AuditLog by the given optional filters, newest first, with pagination.
    /// Never mutates AuditLog -- SELECT only.
    /// </summary>
    Task<AuditLogSearchResultDto> SearchAsync(AuditLogSearchCriteria criteria, CancellationToken cancellationToken);
}
