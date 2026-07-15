// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Display-ready view model for a single key-date entry
//   rendered by ScheduleCalendar.razor (planned/actual start, planned/actual
//   end, target completion, milestone target dates).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.PlanningProjects.Models;

/// <summary>Discriminates the kind of schedule event shown on the ScheduleCalendar component.</summary>
public enum ScheduleEventKind
{
    /// <summary>Planned/target start date.</summary>
    PlannedStart,

    /// <summary>Planned/target completion date.</summary>
    PlannedEnd,

    /// <summary>Actual recorded start date.</summary>
    ActualStart,

    /// <summary>Actual recorded completion date.</summary>
    ActualEnd,

    /// <summary>A project milestone target date.</summary>
    Milestone,
}

/// <summary>Display-ready key-date entry for the ScheduleCalendar component.</summary>
public sealed record ScheduleEventViewModel
{
    /// <summary>Gets the event label shown in the calendar list.</summary>
    public required string Label { get; init; }

    /// <summary>Gets the event date (UTC).</summary>
    public required DateTimeOffset DateUtc { get; init; }

    /// <summary>Gets the kind of schedule event.</summary>
    public required ScheduleEventKind Kind { get; init; }

    /// <summary>Gets a value indicating whether this date has already occurred without the paired actual being recorded (i.e., overdue).</summary>
    public bool IsOverdue { get; init; }
}
