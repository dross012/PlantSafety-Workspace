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
using System.ComponentModel.DataAnnotations;

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

/// <summary>
/// Request body for creating a new Near Miss record. Mirrors
/// PlantSafety.Api.Features.SafetyEvents.NearMisses.Contracts.Requests.CreateNearMissRequest,
/// with DataAnnotations mirroring CreateNearMissCommandValidator's rules.
/// Backed by POST v1/features/safety-events/near-misses.
/// </summary>
public sealed class CreateNearMissRequest
{
    /// <summary>Site where the near miss occurred.</summary>
    [Required(ErrorMessage = "Site is required.")]
    public Guid? SiteId { get; set; }

    /// <summary>Short title. Max 200 characters.</summary>
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title must be 200 characters or fewer.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Narrative description. Max 4000 characters.</summary>
    [Required(ErrorMessage = "Description is required.")]
    [StringLength(4000, ErrorMessage = "Description must be 4000 characters or fewer.")]
    public string Description { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the near miss occurred. Cannot be in the future.</summary>
    [Required(ErrorMessage = "Occurred date is required.")]
    public DateTimeOffset? OccurredAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Potential severity if the event had escalated (Minor, Moderate, Serious, Critical).</summary>
    [Required(ErrorMessage = "Potential severity is required.")]
    public string PotentialSeverity { get; set; } = "Minor";

    /// <summary>Hazard category.</summary>
    [Required(ErrorMessage = "Hazard category is required.")]
    public string HazardCategory { get; set; } = "Other";

    /// <summary>Worker who is reporting the near miss.</summary>
    [Required(ErrorMessage = "Reporting worker is required.")]
    public Guid? ReportedByWorkerId { get; set; }
}

/// <summary>
/// Request body for updating an existing Near Miss. Mirrors
/// PlantSafety.Api.Features.SafetyEvents.NearMisses.Contracts.Requests.UpdateNearMissRequest.
/// Null fields mean "no change". Backed by PUT v1/features/safety-events/near-misses/{id}.
/// </summary>
public sealed class UpdateNearMissRequest
{
    /// <summary>Updated title. Null means no change.</summary>
    [StringLength(200, ErrorMessage = "Title must be 200 characters or fewer.")]
    public string? Title { get; set; }

    /// <summary>Updated description. Null means no change.</summary>
    [StringLength(4000, ErrorMessage = "Description must be 4000 characters or fewer.")]
    public string? Description { get; set; }

    /// <summary>Updated potential severity. Null means no change.</summary>
    public string? PotentialSeverity { get; set; }

    /// <summary>Updated hazard category. Null means no change.</summary>
    public string? HazardCategory { get; set; }

    /// <summary>Optimistic concurrency token from the last read of this Near Miss.</summary>
    public string? ETag { get; set; }
}
