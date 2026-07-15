// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Exports module. Mirrors the real
//   PlantSafety.Api read-model shapes (ExportItemDto / ExportResponse at
//   GET v1/integrations/export) — scheduled/automated data export runs pushed
//   to external integrated systems.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Exports.Models;

/// <summary>Represents a scheduled/automated data export run (mirrors <c>ExportItemDto</c>).</summary>
public sealed class ExportItemDto
{
    /// <summary>Gets the identifier of the scheduled export run.</summary>
    public Guid ExportRunId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the external system the data was exported to.</summary>
    public string TargetSystem { get; init; } = string.Empty;

    /// <summary>Gets the category of data exported (e.g., Incidents, ComplianceRecords, FullDataset).</summary>
    public string ExportType { get; init; } = string.Empty;

    /// <summary>Gets the number of records included in the export.</summary>
    public int RecordCount { get; init; }

    /// <summary>Gets the current export run status (e.g., Queued/Running/Completed/Failed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the export run completed.</summary>
    public DateTime CompletedAtUtc { get; init; }

    /// <summary>Gets the identity of the user or service principal that initiated the export.</summary>
    public Guid InitiatedBy { get; init; }
}

/// <summary>Envelope returned by GET v1/integrations/export (mirrors <c>ExportResponse</c>).</summary>
public sealed class ExportsResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the export run items in the requested window.</summary>
    public IReadOnlyList<ExportItemDto> Items { get; init; } = [];
}
