// ============================================================================
//  Project:        Shared.AuditLogging
//  Module:         Abstractions.Models
//  Layer:          Abstractions
//  File:           AuditLogSearchResult.cs
//  Created:        2026-07-26
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Paginated result envelope for AuditLog search results. Originated as
//                  PlantSafety.Application's Operations.AuditViewer.Models.
//                  AuditLogSearchResultDto (created 2026-07-20); extracted here 2026-07-26
//                  and renamed to drop the "Dto" suffix, matching this record's role as a
//                  shared, cross-module contract rather than a single layer's transport
//                  shape. Additive-only evolution.
// ============================================================================
namespace Shared.AuditLogging.Abstractions.Models;

/// <summary>A single page of AuditLog search results plus paging metadata.</summary>
/// <param name="Items">The audit log entries for the requested page.</param>
/// <param name="PageNumber">The 1-based page number returned.</param>
/// <param name="PageSize">The page size requested.</param>
/// <param name="TotalCount">The total number of rows matching the filter, across all pages.</param>
public sealed record AuditLogSearchResult(
    IReadOnlyList<AuditLogEntry> Items,
    int PageNumber,
    int PageSize,
    int TotalCount);
