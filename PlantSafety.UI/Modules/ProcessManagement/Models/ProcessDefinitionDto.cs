// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-15
// FILE DESCRIPTION: Client-side DTOs for the Process Management module. Mirrors
//   ProcessResponse/ProcessListResponse at GET v1/features/process-management/processes,
//   the first real backend surface for this module (previously there was no
//   Domain/Application/Api surface at all).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.ProcessManagement.Models;

/// <summary>Represents a single Process Definition record (mirrors <c>ProcessResponse</c>).</summary>
public sealed class ProcessDefinitionDto
{
    /// <summary>Gets the process identifier.</summary>
    public Guid ProcessId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the human-readable process name.</summary>
    public string ProcessName { get; init; } = string.Empty;

    /// <summary>Gets the safety-critical process category.</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Gets the lifecycle status (Draft/Active/UnderReview/Retired).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the monotonically increasing revision number.</summary>
    public int Version { get; init; }

    /// <summary>Gets the identity of the accountable process owner.</summary>
    public Guid OwnerId { get; init; }

    /// <summary>Gets the UTC timestamp of last modification.</summary>
    public DateTimeOffset LastModifiedAtUtc { get; init; }
}

/// <summary>Paginated envelope returned by GET v1/features/process-management/processes (mirrors <c>ProcessListResponse</c>).</summary>
public sealed class ProcessListResponseDto
{
    /// <summary>Gets the process definitions in the current page.</summary>
    public IReadOnlyList<ProcessDefinitionDto> Items { get; init; } = [];

    /// <summary>Gets the total matching record count across all pages.</summary>
    public int TotalCount { get; init; }

    /// <summary>Gets the current page number.</summary>
    public int PageNumber { get; init; }

    /// <summary>Gets the page size.</summary>
    public int PageSize { get; init; }

    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages { get; init; }
}
