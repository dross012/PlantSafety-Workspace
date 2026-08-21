// ============================================================================
//  Project:        Shared.AuditLogging
//  Module:         Abstractions.Models
//  Layer:          Abstractions
//  File:           AuditLogSearchCriteria.cs
//  Created:        2026-07-26
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Search/filter/pagination criteria for querying
//                  [PlantSafety.AuditDB].dbo.AuditLog. All filters are optional and
//                  additive (AND-combined); null means "no filter on this column".
//                  Originated as PlantSafety.Application's Operations.AuditViewer.Models.
//                  AuditLogSearchCriteria (created 2026-07-20); extracted here 2026-07-26.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.AuditLogging.Abstractions.Models;

/// <summary>Optional filter + pagination criteria for searching the shared AuditLog table.</summary>
/// <param name="TableName">Optional exact match on the audited table/entity name.</param>
/// <param name="Operation">Optional exact match on the audited operation ("INSERT", "UPDATE", "DELETE", "VIEW").</param>
/// <param name="ChangedBy">Optional exact match on the SQL principal that performed the operation.</param>
/// <param name="Module">Optional exact match on the stored procedure/module name.</param>
/// <param name="Severity">Optional exact match on numeric severity.</param>
/// <param name="FromUtc">Optional inclusive lower bound on ChangeTime (UTC).</param>
/// <param name="ToUtc">Optional inclusive upper bound on ChangeTime (UTC).</param>
/// <param name="SearchText">
/// Optional free-text match against ErrorContext (the field that carries change-detail
/// key/value pairs -- see AuditLogEntry's diff-view scope note).
/// </param>
/// <param name="PageNumber">1-based page number. Defaults to 1.</param>
/// <param name="PageSize">Maximum rows per page. Defaults to 50.</param>
public sealed record AuditLogSearchCriteria(
    string? TableName = null,
    string? Operation = null,
    string? ChangedBy = null,
    string? Module = null,
    int? Severity = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null,
    string? SearchText = null,
    int PageNumber = 1,
    int PageSize = 50);
