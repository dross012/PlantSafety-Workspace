// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Module-scoped constants for the Analytics module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Analytics.Constants;

/// <summary>Module-scoped constants for Analytics.</summary>
public static class AnalyticsModuleConstants
{
    /// <summary>Canonical module name.</summary>
    public const string ModuleName = "Analytics";

    /// <summary>SQL schema name.</summary>
    public const string SqlSchema = "Analytics";

    /// <summary>Cache key prefix for dashboard snapshots.</summary>
    public const string DashboardCachePrefix = "dashboard";

    /// <summary>Dashboard cache expiry in minutes.</summary>
    public const int DashboardCacheExpiryMinutes = 15;

    /// <summary>Culture index score range minimum.</summary>
    public const decimal CultureScoreMin = 0m;

    /// <summary>Culture index score range maximum.</summary>
    public const decimal CultureScoreMax = 10m;
}
