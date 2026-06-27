// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Validation constants specific to the Analytics module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Analytics.Validation;

/// <summary>Validation constraint constants for Analytics features.</summary>
public static class AnalyticsValidationConstants
{
    /// <summary>Minimum date range in days for safety reports.</summary>
    public const int MinReportRangeDays = 1;

    /// <summary>Maximum date range in days for a single report query (≈5 years).</summary>
    public const int MaxReportRangeDays = 1_825;

    /// <summary>Dashboard cache expiry in minutes.</summary>
    public const int DashboardCacheExpiryMinutes = 15;
}
