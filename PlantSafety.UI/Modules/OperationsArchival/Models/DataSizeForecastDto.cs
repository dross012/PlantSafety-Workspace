// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-20
// FILE DESCRIPTION: Client-side-only "Data Size Forecast" model. NOT returned by any API
//   endpoint -- computed entirely in the UI from the existing Operations.Backup database
//   size history (GET /api/v1/operations/backup/database-size) via a simple two-point
//   linear extrapolation (earliest snapshot vs latest snapshot). This is a deliberate,
//   documented approximation, not a statistical forecast model, chosen so the Archival
//   module can offer a size-trend indicator without building new size-tracking
//   infrastructure -- see ArchivalDashboardPage.razor.cs BuildForecasts().
// SchemaVersion: 2026.1
// ============================================================

using System;

namespace PlantSafety.UI.Modules.OperationsArchival.Models;

/// <summary>
/// A simple, client-computed database-size growth forecast. See file header for the
/// approximation method -- this is NOT a precise or statistically validated projection.
/// </summary>
public sealed class DataSizeForecastDto
{
    /// <summary>Gets the PlantSafety.* database this forecast applies to.</summary>
    public string DatabaseName { get; init; } = string.Empty;

    /// <summary>Gets the earliest snapshot's total size (sum across all files), in MB.</summary>
    public decimal EarliestSizeMB { get; init; }

    /// <summary>Gets the earliest snapshot's timestamp.</summary>
    public DateTime EarliestCapturedUtc { get; init; }

    /// <summary>Gets the most recent snapshot's total size (sum across all files), in MB.</summary>
    public decimal LatestSizeMB { get; init; }

    /// <summary>Gets the most recent snapshot's timestamp.</summary>
    public DateTime LatestCapturedUtc { get; init; }

    /// <summary>Gets the approximate daily growth rate, in MB/day, derived from (LatestSizeMB - EarliestSizeMB) / days elapsed.</summary>
    public decimal DailyGrowthMB { get; init; }

    /// <summary>Gets the approximate projected total size 30 days from the latest snapshot, in MB.</summary>
    public decimal Projected30DayMB { get; init; }

    /// <summary>Gets the approximate projected total size 90 days from the latest snapshot, in MB.</summary>
    public decimal Projected90DayMB { get; init; }
}
