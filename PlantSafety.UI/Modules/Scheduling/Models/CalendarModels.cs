// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Scheduling Calendar page
//   (Volume S1 §3) — a 7-column week grid of job chips.
// SchemaVersion: 2026.1
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Scheduling.Models;

/// <summary>A single job chip placed on the calendar grid.</summary>
public sealed class CalendarJobDto
{
    /// <summary>Gets the job identifier.</summary>
    public Guid JobId { get; init; }

    /// <summary>Gets the human-readable job code.</summary>
    public string JobCode { get; init; } = string.Empty;

    /// <summary>Gets the assigned resource display name.</summary>
    public string ResourceName { get; init; } = string.Empty;

    /// <summary>Gets the chip start time (UTC).</summary>
    public DateTimeOffset StartUtc { get; init; }

    /// <summary>Gets the chip end time (UTC).</summary>
    public DateTimeOffset EndUtc { get; init; }

    /// <summary>Gets the job status code.</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>One calendar day column containing its job chips.</summary>
public sealed class CalendarDayDto
{
    /// <summary>Gets the calendar date for this column.</summary>
    public DateOnly Date { get; init; }

    /// <summary>Gets the jobs scheduled on this day.</summary>
    public IReadOnlyList<CalendarJobDto> Jobs { get; init; } = [];
}

/// <summary>A full 7-day (Sun-Sat) calendar week.</summary>
public sealed class CalendarWeekDto
{
    /// <summary>Gets the first date (Sunday) of the displayed week.</summary>
    public DateOnly WeekStart { get; init; }

    /// <summary>Gets the 7 day columns, Sunday through Saturday.</summary>
    public IReadOnlyList<CalendarDayDto> Days { get; init; } = [];
}
