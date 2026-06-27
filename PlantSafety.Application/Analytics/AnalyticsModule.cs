// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Analytics module registration marker and constants.
//   Covers: Culture Analytics, Dashboard Analytics, Engagement Analytics, Predictive Analytics,
//           Reporting Analytics, Scheduling Analytics, Scorecards, and Trend Analysis.
//   All commands, queries, and handlers for this module are in Features/Analytics.
// SchemaVersion: 1
// Additive-only evolution: new constants may be added; no existing constants may be renamed or removed.

namespace PlantSafety.Application.Analytics;

/// <summary>
/// Module marker and constants for the Analytics bounded context.
/// Covers KPI dashboards, culture scoring, engagement tracking, predictive safety, and trend analysis.
/// Analytics is a read-heavy module; most operations are queries with no state mutation.
/// </summary>
public static class AnalyticsModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "Analytics";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "Analytics";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "Analytics";

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to view analytics dashboards.</summary>
        public const string ViewDashboard = "Analytics.Dashboard.View";

        /// <summary>Permission to view culture analytics.</summary>
        public const string ViewCulture = "Analytics.Culture.View";

        /// <summary>Permission to view predictive analytics.</summary>
        public const string ViewPredictive = "Analytics.Predictive.View";

        /// <summary>Permission to export analytics reports.</summary>
        public const string ExportAnalytics = "Analytics.Reports.Export";

        /// <summary>Permission to view scorecard results.</summary>
        public const string ViewScoreCards = "Analytics.ScoreCards.View";
    }
}
