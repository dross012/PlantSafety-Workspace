// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-04
// FILE DESCRIPTION: Client-side DTO for a single AccumulationArea read-model record, as returned
//   by the "nearing time limit" query. Mirrors PlantSafety.Application.HazardousWasteManagement.
//   AccumulationAreas.Models.AccumulationAreaItemDto field-for-field. Display only.
// SchemaVersion: 2026.2
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.HazardousWasteManagement.Models;

/// <summary>A single hazardous waste accumulation area record, as surfaced on the Time-Limit Board.</summary>
public sealed class AccumulationAreaDto
{
    /// <summary>Gets the accumulation area record identifier.</summary>
    public Guid AccumulationAreaId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the human-readable name of the accumulation area.</summary>
    public string AreaName { get; init; } = string.Empty;

    /// <summary>Gets the current lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the default RCRA-style accumulation time limit in days.</summary>
    public int TimeLimitDays { get; init; }

    /// <summary>Gets the earliest regulatory deadline among the area's open containers, if any.</summary>
    public DateTime? EarliestContainerDeadlineUtc { get; init; }
}
