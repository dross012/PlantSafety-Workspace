// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side item DTO for the Scheduling Analytics module.
//   Mirrors PlantSafety.Application.Analytics.Schedules.Models.SchedulesItemDto
//   field-for-field so SchedulingAnalyticsApiClient can deserialize the real
//   backend response from api/v1/analytics/schedules directly. Display-only
//   — no domain logic.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.SchedulingAnalytics.Models;

/// <summary>A single scheduled analytics job or recurring report definition (mirrors <c>SchedulesItemDto</c>).</summary>
public sealed class ScheduleEntryDto
{
    /// <summary>Gets the schedule item identifier.</summary>
    public Guid ScheduleItemId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the schedule name.</summary>
    public string ScheduleName { get; init; } = string.Empty;

    /// <summary>Gets the recurrence frequency (e.g., Daily, Weekly, Monthly).</summary>
    public string Frequency { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp of the next scheduled run.</summary>
    public DateTime NextRunAtUtc { get; init; }

    /// <summary>Gets the current status (e.g., Active, Paused, Disabled).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the schedule was created.</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Gets the identity of the user who created the schedule.</summary>
    public Guid CreatedBy { get; init; }
}
