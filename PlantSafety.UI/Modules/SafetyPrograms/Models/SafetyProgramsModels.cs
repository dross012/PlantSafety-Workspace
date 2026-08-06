// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Safety Programs module. Covers
//   the Safety Programs List and Program Detail pages using the generic
//   list/detail template (no dedicated architecture manual for this module).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlantSafety.UI.Modules.SafetyPrograms.Models;

/// <summary>Lightweight list-view DTO for a safety program.</summary>
public sealed class SafetyProgramListItemDto
{
    /// <summary>Gets the program identifier (UUIDv7).</summary>
    public Guid ProgramId { get; init; }

    /// <summary>Gets the program name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the program category (e.g. Training, Behavioral, Compliance, Recognition).</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Gets the program status (Active, Planned, OnHold, Completed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the participation percentage (0-100).</summary>
    public double ParticipationPercent { get; init; }

    /// <summary>Gets the program owner display name.</summary>
    public string OwnerName { get; init; } = string.Empty;
}

/// <summary>Full safety program detail DTO.</summary>
public sealed class SafetyProgramDetailDto
{
    /// <summary>Gets the program identifier.</summary>
    public Guid ProgramId { get; init; }

    /// <summary>Gets the program name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the program description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the program category.</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Gets the program status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the program start date.</summary>
    public DateOnly StartDate { get; init; }

    /// <summary>Gets the program end date, if any.</summary>
    public DateOnly? EndDate { get; init; }

    /// <summary>Gets the participation percentage (0-100).</summary>
    public double ParticipationPercent { get; init; }

    /// <summary>Gets the enrolled participant count.</summary>
    public int EnrolledCount { get; init; }

    /// <summary>Gets the program owner display name.</summary>
    public string OwnerName { get; init; } = string.Empty;

    /// <summary>Gets the associated program goals/objectives.</summary>
    public IReadOnlyList<string> Goals { get; init; } = [];

    /// <summary>
    /// Gets the live rollup of related activity in the SafetyPolicies/SafetyProcedures/
    /// SafetyCulture modules for this program's Category, composed server-side at query time.
    /// Empty for programs whose Category has no curated composition mapping.
    /// </summary>
    public IReadOnlyList<SafetyProgramLinkedActivityDto> LinkedActivity { get; init; } = [];
}

/// <summary>A single composed rollup section of a safety program's live linked activity.</summary>
public sealed class SafetyProgramLinkedActivityDto
{
    /// <summary>Gets the sibling module this section was composed from.</summary>
    public string SourceModule { get; init; } = string.Empty;

    /// <summary>Gets the specific sub-domain/query within the source module.</summary>
    public string SourceCategory { get; init; } = string.Empty;

    /// <summary>Gets the total matching item count, or null when unavailable.</summary>
    public int? ItemCount { get; init; }

    /// <summary>Gets the count of active items, or null when unavailable.</summary>
    public int? ActiveItemCount { get; init; }

    /// <summary>Gets a value indicating whether this section's live data was retrieved successfully.</summary>
    public bool IsAvailable { get; init; }

    /// <summary>Gets a sanitized reason this section is unavailable, if applicable.</summary>
    public string? UnavailableReason { get; init; }
}

/// <summary>
/// Request body for creating a new Safety Program. Mirrors
/// PlantSafety.Api.SafetyPrograms.Contracts.Requests.CreateSafetyProgramRequest, with
/// DataAnnotations mirroring CreateSafetyProgramCommandValidator's rules (Name required,
/// max 256 chars; Category required; EndDate must not precede StartDate). Backed by
/// POST v1/safety-programs.
/// </summary>
public sealed class CreateSafetyProgramRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; set; }

    /// <summary>Identity of the accountable program owner.</summary>
    [Required(ErrorMessage = "Owner is required.")]
    public Guid OwnerId { get; set; }

    /// <summary>Program name.</summary>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(256, ErrorMessage = "Name must be 256 characters or fewer.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Program description.</summary>
    public string? Description { get; set; }

    /// <summary>Program category (Training/Behavioral/Compliance/Recognition).</summary>
    [Required(ErrorMessage = "Category is required.")]
    public string Category { get; set; } = "Training";

    /// <summary>Program start date.</summary>
    [Required(ErrorMessage = "Start date is required.")]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>Optional program end date. Must not precede StartDate.</summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>Initial goals/objectives, if any. Populated from <see cref="GoalsText"/> before submission.</summary>
    public IReadOnlyList<string>? Goals { get; set; }

    /// <summary>
    /// UI-only free-text goals/objectives entry, one per line. Parsed into <see cref="Goals"/> by the
    /// Create Safety Program page before the request is serialized; not part of the backend contract.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public string? GoalsText { get; set; }
}

/// <summary>Response payload after creating a new Safety Program (POST v1/safety-programs).</summary>
public sealed class CreateSafetyProgramResponse
{
    /// <summary>Gets the identifier of the newly created safety program.</summary>
    public Guid SafetyProgramId { get; init; }

    /// <summary>Gets the initial lifecycle status of the newly created program.</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>Request body for POST v1/safety-programs/{safetyProgramId}/status.</summary>
public sealed class ChangeSafetyProgramStatusRequest
{
    /// <summary>The target status (Planned/Active/OnHold/Completed).</summary>
    [Required]
    public string NewStatus { get; set; } = string.Empty;
}

/// <summary>Response payload after a status transition (POST v1/safety-programs/{safetyProgramId}/status).</summary>
public sealed class ChangeSafetyProgramStatusResponse
{
    /// <summary>Gets the safety program identifier.</summary>
    public Guid SafetyProgramId { get; init; }

    /// <summary>Gets the new lifecycle status after the transition.</summary>
    public string Status { get; init; } = string.Empty;
}
