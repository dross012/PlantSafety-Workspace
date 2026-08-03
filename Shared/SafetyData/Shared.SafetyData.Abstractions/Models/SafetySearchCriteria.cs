// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models
//  Layer:          Abstractions
//  File:           SafetySearchCriteria.cs
//  Created:        2026-07-27
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Search/filter/pagination criteria shared by all three
//                  Shared.SafetyData read paths (Hazard Summary, Corrective
//                  Action, Safety Compliance). Unlike Shared.AuditLogging's
//                  AuditLogSearchCriteria (dbo.AuditLog is server-level,
//                  cross-database data), the tables backing this module
//                  ([RiskMgmt].[Risk], [RiskMgmt].[ControlMeasure],
//                  [ContinuousImprovement].[CAPA]) are all tenant-scoped
//                  (TenantId NOT NULL, discovered by PlantSafety's platform-wide
//                  Row-Level-Security scan -- see RiskManagement\Sql\Tables\
//                  Risk.sql's header comment), so TenantId is a required filter
//                  here, not optional. All other filters are optional and
//                  additive (AND-combined); null means "no filter on this
//                  column".
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models;

/// <summary>Required tenant scope plus optional filter/pagination criteria for Shared.SafetyData queries.</summary>
/// <param name="TenantId">Required tenant scope. Every backing table is tenant-scoped; there is no cross-tenant query path.</param>
/// <param name="SiteId">Optional exact match on site.</param>
/// <param name="FromUtc">Optional inclusive lower bound on the record's date field (CreatedAtUtc for hazards, DueDateUtc for corrective actions).</param>
/// <param name="ToUtc">Optional inclusive upper bound on the record's date field.</param>
/// <param name="PageNumber">1-based page number. Defaults to 1. Ignored by GetSafetyComplianceAsync (rollup, not paginated).</param>
/// <param name="PageSize">Maximum rows per page. Defaults to 50. Ignored by GetSafetyComplianceAsync.</param>
public sealed record SafetySearchCriteria(
    Guid TenantId,
    Guid? SiteId = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null,
    int PageNumber = 1,
    int PageSize = 50);
