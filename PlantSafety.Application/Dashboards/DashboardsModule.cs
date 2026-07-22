// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Module marker, constants, and onboarding documentation for the Dashboards
//   bounded context.
// SchemaVersion: 1
// Additive-only evolution: new constants may be added; no existing constants may be renamed or removed.

namespace PlantSafety.Application.Dashboards;

/// <summary>
/// Module marker, constants, and onboarding documentation for the Dashboards bounded context.
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> Owns the visual and data-aggregation layer of the platform — role-based
/// dashboard pages composed of reusable widgets (charts, graphs, gauges, tables, heatmaps, maps,
/// timelines, trend graphs, KPI cards) that surface read models exposed by every other domain
/// module. Grounded in <c>docs/reference/PlantSafety_Program_Structure_extract.txt</c>,
/// "Dashboard Design Architecture", "FULL PROGRAM DASHBOARD LIST" (19 named dashboards), and
/// "Appendix E — Dashboard Widget Library".</para>
///
/// <para><b>Responsibilities:</b> Define and version dashboard layouts (name, description,
/// category, primary role, default filters, widget placements); define and version reusable
/// widget configurations (type, data source, refresh interval, parameters, permitted roles);
/// enforce role-based dashboard/widget visibility; model (but not render) the widget data
/// contract (Series[]/Categories[]/Metrics[]) other modules populate.</para>
///
/// <para><b>Dependencies:</b> None on other bounded-context internals (Maximalist Module Design,
/// Part IV). Widgets reference source modules only by canonical module name + query/endpoint
/// identifier (<c>WidgetDataSource</c>), never by direct type reference.</para>
///
/// <para><b>Inputs:</b> Read-model query results and endpoint data produced by every other domain
/// module (Alerts, Risk Management, PPE, Machine Safety, Permits, Scheduling, Observability,
/// Culture, Scorecards, Training, Contractors, Checklists, Security, Environmental, Quality),
/// per "1. Data Sources (Domain Modules)".</para>
///
/// <para><b>Outputs:</b> Resolved dashboard layouts and per-widget data contracts for the
/// Presentation/UI layer, served via <c>GET /dashboard/{id}/widgets</c> and
/// <c>GET /widget/{id}/data</c>-shaped queries (per "Dashboard API Queries").</para>
///
/// <para><b>Events emitted:</b> <c>Dashboards.DashboardCreated</c>, <c>Dashboards.DashboardPublished</c>,
/// <c>Dashboards.DashboardArchived</c>, <c>Dashboards.DashboardWidgetAdded</c>,
/// <c>Dashboards.DashboardWidgetRemoved</c>, <c>Dashboards.WidgetCreated</c>,
/// <c>Dashboards.WidgetActivated</c>, <c>Dashboards.WidgetConfigured</c>,
/// <c>Dashboards.WidgetDeprecated</c>.</para>
///
/// <para><b>Events consumed:</b> None directly. Widget data is resolved synchronously through
/// read-model queries against each source module at render time, not through cross-module
/// integration events, to keep dashboards eventually-fresh without coupling to another module's
/// internal event shapes.</para>
///
/// <para><b>Integration points:</b> Every other domain module (as a widget data source);
/// Alerts module (Alerts &amp; Emergency Response Dashboard, real-time push widgets); UI/API
/// layer (dashboard/widget resolution endpoints, not generated in this Domain+Application-only
/// pass).</para>
/// </remarks>
public static class DashboardsModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "Dashboards";

    /// <summary>The SQL schema name owned by this module (reserved for future Infrastructure work).</summary>
    public const string SqlSchema = "Dashboards";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "Dashboards";

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to create a dashboard.</summary>
        public const string CreateDashboard = "Dashboards.Dashboard.Create";

        /// <summary>Permission to publish a dashboard.</summary>
        public const string PublishDashboard = "Dashboards.Dashboard.Publish";

        /// <summary>Permission to archive a dashboard.</summary>
        public const string ArchiveDashboard = "Dashboards.Dashboard.Archive";

        /// <summary>Permission to modify a dashboard's widget layout.</summary>
        public const string ManageDashboardLayout = "Dashboards.Dashboard.ManageLayout";

        /// <summary>Permission to create or configure a widget.</summary>
        public const string ManageWidget = "Dashboards.Widget.Manage";

        /// <summary>Permission to view published dashboards.</summary>
        public const string ViewDashboard = "Dashboards.Dashboard.View";
    }
}
