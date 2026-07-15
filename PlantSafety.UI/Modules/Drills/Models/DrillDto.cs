// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTO for a Safety Drill / Exercise record.
//   Mirrors PlantSafety.Application.Features.SafetyEvents.Models
//   .DrillsAndExercises.DrillDto. The backend currently exposes a Create
//   (schedule) endpoint only — List/GetById are modeled against this
//   best-inferable DTO shape.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

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
