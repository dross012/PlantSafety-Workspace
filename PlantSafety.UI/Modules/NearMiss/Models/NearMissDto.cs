// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTO for a Near Miss record. Mirrors
//   PlantSafety.Application.Features.SafetyEvents.Models.NearMisses.NearMissDto
//   returned by GET /v1/features/safety-events/near-misses[/{id}]. Used for both
//   list and detail views — the API projects the same shape for both.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.NearMiss.Models;

/// <summary>Read-side projection of a Near Miss record.</summary>
public sealed class NearMissDto
{
    /// <summary>Gets the unique near miss identifier (UUIDv7).</summary>
    public Guid NearMissId { get; init; }

    /// <summary>Gets the owning tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site where the near miss occurred.</summary>
    public Guid SiteId { get; init; }

    /// <summary>Gets the short descriptive title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the narrative of what happened and what was avoided.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the near miss occurred.</summary>
    public DateTimeOffset OccurredAtUtc { get; init; }

    /// <summary>Gets the potential severity if the event had escalated.</summary>
    public string PotentialSeverity { get; init; } = string.Empty;

    /// <summary>Gets the hazard category (e.g., Chemical, Mechanical, Ergonomic).</summary>
    public string HazardCategory { get; init; } = string.Empty;

    /// <summary>Gets the current workflow status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the worker who reported the near miss.</summary>
    public Guid ReportedByWorkerId { get; init; }

    /// <summary>Gets the UTC timestamp of last modification.</summary>
    public DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Gets the row version for concurrency.</summary>
    public string ETag { get; init; } = string.Empty;
}
