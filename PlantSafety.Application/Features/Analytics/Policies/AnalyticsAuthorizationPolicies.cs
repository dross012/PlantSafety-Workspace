// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Authorization policy names for the Analytics module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Analytics.Policies;

/// <summary>Authorization policy name constants for Analytics features.</summary>
public static class AnalyticsAuthorizationPolicies
{
    /// <summary>Policy that grants access to view the safety dashboard.</summary>
    public const string CanViewDashboard = "Analytics.CanViewDashboard";

    /// <summary>Policy that grants access to view safety reports.</summary>
    public const string CanViewReports = "Analytics.CanViewReports";

    /// <summary>Policy that grants access to export report data.</summary>
    public const string CanExportReports = "Analytics.CanExportReports";

    /// <summary>Policy that grants access to view the safety culture index.</summary>
    public const string CanViewCultureIndex = "Analytics.CanViewCultureIndex";

    /// <summary>Policy that grants access to predictive analytics.  Requires Intelligence module licence.</summary>
    public const string CanViewPredictiveInsights = "Analytics.CanViewPredictiveInsights";
}
