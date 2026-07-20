// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-20
// FILE DESCRIPTION: Client-side DTOs for the Operations.AuditViewer module. Mirror
//   PlantSafety.Application.Operations.AuditViewer.Models.* returned by
//   PlantSafety.Api.Operations.AuditViewer.Controllers.V1
//   .OperationsAuditViewerV1Controller. Surfaces the shared, cross-database
//   [PlantSafety.AuditDB].dbo.AuditLog table -- real platform audit infrastructure.
//   Read-only: no model here supports a write/mutate operation.
//
//   DIFF VIEW SCOPE NOTE: BeforeSnapshot/AfterSnapshot are real AuditLog columns, but
//   every stored procedure in this codebase currently writes them as null. The "Change
//   Detail" panel on AuditViewerDashboardPage therefore parses ErrorContext (typically
//   "Key=Value; Key=Value" pairs) into readable rows instead of presenting a true
//   structured before/after diff -- see AuditLogEntryDto.cs for the full rationale.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.OperationsAuditViewer.Models;

/// <summary>A single row from the shared [PlantSafety.AuditDB].dbo.AuditLog table.</summary>
public sealed class AuditLogEntryDto
{
    /// <summary>Gets the audited table/entity name.</summary>
    public string TableName { get; init; } = string.Empty;

    /// <summary>Gets the database the write originated from.</summary>
    public string SourceDatabase { get; init; } = string.Empty;

    /// <summary>Gets the audited operation ("INSERT", "UPDATE", "DELETE", "VIEW").</summary>
    public string Operation { get; init; } = string.Empty;

    /// <summary>Gets the SQL principal that performed the operation.</summary>
    public string ChangedBy { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp the audit row was written.</summary>
    public DateTime ChangeTime { get; init; }

    /// <summary>Gets a value indicating whether the audited operation was a read/view.</summary>
    public bool ViewerAccessed { get; init; }

    /// <summary>Gets the free-text change-detail string (typically "Key=Value; Key=Value" pairs).</summary>
    public string? ErrorContext { get; init; }

    /// <summary>Gets the numeric severity.</summary>
    public int Severity { get; init; }

    /// <summary>Gets the stored procedure/module that produced the row.</summary>
    public string Module { get; init; } = string.Empty;

    /// <summary>Gets the reserved "before" snapshot column. Always null in every current write path.</summary>
    public string? BeforeSnapshot { get; init; }

    /// <summary>Gets the reserved "after" snapshot column. Always null in every current write path.</summary>
    public string? AfterSnapshot { get; init; }
}

/// <summary>A single page of AuditLog search results plus paging metadata.</summary>
public sealed class AuditLogSearchResultDto
{
    /// <summary>Gets the audit log entries for the requested page.</summary>
    public IReadOnlyList<AuditLogEntryDto> Items { get; init; } = [];

    /// <summary>Gets the 1-based page number returned.</summary>
    public int PageNumber { get; init; }

    /// <summary>Gets the page size requested.</summary>
    public int PageSize { get; init; }

    /// <summary>Gets the total number of rows matching the filter, across all pages.</summary>
    public int TotalCount { get; init; }
}

/// <summary>Client-side filter state for an AuditLog search, mirrored 1:1 to the API's query-string filters.</summary>
public sealed class AuditLogFilter
{
    /// <summary>Gets or sets the exact table/entity name filter.</summary>
    public string? TableName { get; set; }

    /// <summary>Gets or sets the exact operation filter.</summary>
    public string? Operation { get; set; }

    /// <summary>Gets or sets the exact "changed by" principal filter.</summary>
    public string? ChangedBy { get; set; }

    /// <summary>Gets or sets the exact module/stored-procedure filter.</summary>
    public string? Module { get; set; }

    /// <summary>Gets or sets the exact severity filter.</summary>
    public int? Severity { get; set; }

    /// <summary>Gets or sets the inclusive lower bound on ChangeTime (UTC).</summary>
    public DateTime? FromUtc { get; set; }

    /// <summary>Gets or sets the inclusive upper bound on ChangeTime (UTC).</summary>
    public DateTime? ToUtc { get; set; }

    /// <summary>Gets or sets a free-text match against ErrorContext.</summary>
    public string? SearchText { get; set; }

    /// <summary>Gets or sets the 1-based page number.</summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>Gets or sets the page size.</summary>
    public int PageSize { get; set; } = 50;
}

/// <summary>A single parsed "Key=Value" pair from ErrorContext, for the "Change Detail" panel.</summary>
/// <param name="Key">The parsed key.</param>
/// <param name="Value">The parsed value.</param>
public sealed record ChangeDetailField(string Key, string Value);
