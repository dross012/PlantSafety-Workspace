// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Permission constants for the Analytics module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Analytics.Permissions;

/// <summary>Permission strings for the Analytics module.  Format: <c>Module.Feature.Action</c>.</summary>
public static class AnalyticsPermissions
{
    /// <summary>Dashboard permissions.</summary>
    public static class Dashboard
    {
        /// <summary>View the safety dashboard.</summary>
        public const string View = "Analytics.Dashboard.View";
    }

    /// <summary>Reporting permissions.</summary>
    public static class Reporting
    {
        /// <summary>View safety reports.</summary>
        public const string View = "Analytics.Reporting.View";
        /// <summary>Export safety reports.</summary>
        public const string Export = "Analytics.Reporting.Export";
    }

    /// <summary>Culture index permissions.</summary>
    public static class CultureIndex
    {
        /// <summary>View the safety culture index.</summary>
        public const string View = "Analytics.CultureIndex.View";
    }

    /// <summary>Predictive analytics permissions.</summary>
    public static class Predictive
    {
        /// <summary>View predictive risk insights.</summary>
        public const string View = "Analytics.Predictive.View";
    }
}
