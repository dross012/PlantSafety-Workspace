// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-08-05 — Added CreateDrillRequest/CreateDrillResponseDto,
//   mirroring PlantSafety.Api.Features.SafetyEvents.DrillsAndExercises
//   .Contracts.Requests.CreateDrillRequest and .Responses.CreateDrillResponse,
//   with DataAnnotations mirroring CreateDrillCommandValidator's rules.
// FILE DESCRIPTION: Client-side DTO for a Safety Drill / Exercise record.
//   Mirrors PlantSafety.Application.Features.SafetyEvents.Models
//   .DrillsAndExercises.DrillDto. The backend currently exposes a Create
//   (schedule) endpoint only — List/GetById are modeled against this
//   best-inferable DTO shape.
// SchemaVersion: 2026.2
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace PlantSafety.UI.Modules.Drills.Models;

/// <summary>Read-side projection of a Safety Drill — a scheduled or ad-hoc emergency-readiness exercise.</summary>
public sealed class DrillDto
{
    /// <summary>Gets the unique drill identifier (UUIDv7).</summary>
    public Guid DrillId { get; init; }

    /// <summary>Gets the owning tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site where the drill was conducted.</summary>
    public Guid SiteId { get; init; }

    /// <summary>Gets the short title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the description of objectives and scope.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the drill type (e.g., Evacuation, FireResponse, ChemicalSpill, MedicalEmergency).</summary>
    public string DrillType { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the drill was scheduled.</summary>
    public DateTimeOffset ScheduledAtUtc { get; init; }

    /// <summary>Gets the UTC timestamp when the drill actually occurred, if executed.</summary>
    public DateTimeOffset? ExecutedAtUtc { get; init; }

    /// <summary>Gets the workflow status (e.g., Scheduled, Completed, Cancelled).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the drill coordinator.</summary>
    public Guid CoordinatorId { get; init; }

    /// <summary>Gets the overall score or outcome rating, if evaluated.</summary>
    public int? OutcomeScore { get; init; }

    /// <summary>Gets the UTC timestamp of last modification.</summary>
    public DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Gets the row version for concurrency.</summary>
    public string ETag { get; init; } = string.Empty;
}

/// <summary>
/// Request body for scheduling a new Safety Drill or Exercise. Mirrors
/// PlantSafety.Api.Features.SafetyEvents.DrillsAndExercises.Contracts.Requests.CreateDrillRequest,
/// with DataAnnotations mirroring CreateDrillCommandValidator's rules (SiteId required, Title
/// required and &lt;= 200 chars, DrillType must be one of the valid codes, ScheduledAtUtc must be UTC).
/// Backed by POST v1/features/safety-events/drills.
/// </summary>
public sealed class CreateDrillRequest
{
    /// <summary>Site where the drill will be conducted.</summary>
    [Required(ErrorMessage = "Site is required.")]
    public Guid SiteId { get; set; }

    /// <summary>Short title. Max 200 characters.</summary>
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title must be 200 characters or fewer.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Description of objectives and scope. Max 4000 characters.</summary>
    [Required(ErrorMessage = "Description is required.")]
    [StringLength(4000, ErrorMessage = "Description must be 4000 characters or fewer.")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Drill type. Must be one of the codes recognized by CreateDrillCommandValidator.</summary>
    [Required(ErrorMessage = "Drill type is required.")]
    public string DrillType { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the drill is scheduled to occur.</summary>
    [Required(ErrorMessage = "Scheduled date/time is required.")]
    public DateTimeOffset ScheduledAtUtc { get; set; } = DateTimeOffset.UtcNow.AddDays(7);

    /// <summary>Drill coordinator (user identifier).</summary>
    [Required(ErrorMessage = "Coordinator is required.")]
    public Guid CoordinatorId { get; set; }
}

/// <summary>
/// Response payload for a newly scheduled drill. Mirrors
/// PlantSafety.Api.Features.SafetyEvents.DrillsAndExercises.Contracts.Responses.CreateDrillResponse.
/// </summary>
public sealed class CreateDrillResponseDto
{
    /// <summary>Gets the identifier of the newly scheduled drill.</summary>
    public Guid DrillId { get; init; }
}
