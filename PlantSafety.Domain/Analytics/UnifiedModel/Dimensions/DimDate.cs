// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.UnifiedModel
//  Layer:          Domain
//  File:           DimDate.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Static calendar dimension. Not slowly-changing -- one immutable row per
//                  calendar date, generated once for the platform's supported date range.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.UnifiedModel.Dimensions;

/// <summary>
/// Static calendar dimension row. One immutable row per calendar date; never revised, so it
/// intentionally does not implement <see cref="Abstractions.ISlowlyChangingDimension"/>.
/// </summary>
public sealed record DimDate
{
    /// <summary>Surrogate key of this date row. Conventionally the date in "yyyyMMdd" integer form, stored as a stable GUID mapping.</summary>
    public required Guid DateKey { get; init; }

    /// <summary>The calendar date this row represents.</summary>
    public required DateOnly CalendarDate { get; init; }

    /// <summary>ISO 8601 year.</summary>
    public required int Year { get; init; }

    /// <summary>Calendar quarter (1-4).</summary>
    public required int Quarter { get; init; }

    /// <summary>Calendar month (1-12).</summary>
    public required int Month { get; init; }

    /// <summary>Month display name.</summary>
    public required string MonthName { get; init; }

    /// <summary>ISO 8601 week number.</summary>
    public required int IsoWeek { get; init; }

    /// <summary>Day of week (1 = Monday .. 7 = Sunday, ISO 8601).</summary>
    public required int DayOfWeekIso { get; init; }

    /// <summary>True for Saturday/Sunday.</summary>
    public required bool IsWeekend { get; init; }

    /// <summary>True when this date is a recognized plant holiday.</summary>
    public bool IsHoliday { get; init; }

    /// <summary>Fiscal year, if the tenant's fiscal calendar differs from the calendar year.</summary>
    public int? FiscalYear { get; init; }

    /// <summary>Fiscal period (1-12 or 1-13), if applicable.</summary>
    public int? FiscalPeriod { get; init; }
}
