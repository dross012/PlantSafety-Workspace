// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.AuditViewer
//  Layer:          Application
//  File:           AuditLogSearchResultDto.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Paginated result envelope for AuditLog search results.
//                  Additive-only evolution.
// ============================================================================
using System.Collections.Generic;

namespace PlantSafety.Application.Operations.AuditViewer.Models;

/// <summary>A single page of AuditLog search results plus paging metadata.</summary>
/// <param name="Items">The audit log entries for the requested page.</param>
/// <param name="PageNumber">The 1-based page number returned.</param>
/// <param name="PageSize">The page size requested.</param>
/// <param name="TotalCount">The total number of rows matching the filter, across all pages.</param>
public sealed record AuditLogSearchResultDto(
    IReadOnlyList<AuditLogEntryDto> Items,
    int PageNumber,
    int PageSize,
    int TotalCount);
