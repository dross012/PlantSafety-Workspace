// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTO for a Safety Observation record. Mirrors
//   PlantSafety.Application.Features.SafetyEvents.Models.Observation.ObservationDto
//   returned by GET /v1/features/safety-events/observations/{id}.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace PlantSafety.UI.Modules.Observations.Models;

/// <summary>Read-side projection of a Safety Observation — a documented workplace safety condition.</summary>
public sealed class ObservationDto
{
    /// <summary>Gets the unique observation identifier (UUIDv7).</summary>
    public Guid ObservationId { get; init; }

    /// <summary>Gets the owning tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site where the observation was made.</summary>
    public Guid SiteId { get; init; }

    /// <summary>Gets the short descriptive title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the narrative description of the observed condition.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the observation type: Positive, AtRisk, Unsafe.</summary>
    public string ObservationType { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the observation was made.</summary>
    public DateTimeOffset ObservedAtUtc { get; init; }

    /// <summary>Gets the worker who made the observation.</summary>
    public Guid ObservedByWorkerId { get; init; }

    /// <summary>Gets the current workflow status (e.g., Open, ActionRequired, Closed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp of last modification.</summary>
    public DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Gets the row version for concurrency.</summary>
    public string ETag { get; init; } = string.Empty;
}

/// <summary>
/// Request body for creating a new Safety Observation. Mirrors
/// PlantSafety.Api.Features.SafetyEvents.Observation.Contracts.Requests.CreateObservationRequest,
/// with DataAnnotations mirroring CreateObservationCommandValidator's rules.
/// Backed by POST v1/features/safety-events/observations.
/// </summary>
public sealed class CreateObservationRequest
{
    /// <summary>Site where the observation was made.</summary>
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

    /// <summary>Observation type: Positive, AtRisk, Unsafe.</summary>
    [Required(ErrorMessage = "Observation type is required.")]
    public string ObservationType { get; set; } = "Positive";

    /// <summary>UTC timestamp when the observation was made. Cannot be in the future.</summary>
    [Required(ErrorMessage = "Observed date is required.")]
    public DateTimeOffset? ObservedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Worker who made the observation.</summary>
    [Required(ErrorMessage = "Observing worker is required.")]
    public Guid? ObservedByWorkerId { get; set; }
}

/// <summary>
/// Request body for updating an existing Safety Observation. Mirrors
/// PlantSafety.Api.Features.SafetyEvents.Observation.Contracts.Requests.UpdateObservationRequest.
/// Null fields mean "no change". Backed by PUT v1/features/safety-events/observations/{id}.
/// </summary>
public sealed class UpdateObservationRequest
{
    /// <summary>Updated title. Null means no change.</summary>
    [StringLength(200, ErrorMessage = "Title must be 200 characters or fewer.")]
    public string? Title { get; set; }

    /// <summary>Updated description. Null means no change.</summary>
    [StringLength(4000, ErrorMessage = "Description must be 4000 characters or fewer.")]
    public string? Description { get; set; }

    /// <summary>Updated observation type. Null means no change.</summary>
    public string? ObservationType { get; set; }

    /// <summary>Optimistic concurrency token from the last read of this Observation.</summary>
    public string? ETag { get; set; }
}
