// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Financials module registration marker and constants.
//   Covers: Budgeting, Cost Tracking, Forecasting, Financial Governance, ROI analysis.
//   All commands, queries, and handlers for this module are in Features/Financials.
// SchemaVersion: 1
// Additive-only evolution: new constants may be added; no existing constants may be renamed or removed.

namespace PlantSafety.Application.Financials;

/// <summary>
/// Module marker and constants for the Financials bounded context.
/// Covers budget definition, cost allocation, cost tracking, forecasting, and financial compliance.
/// Budget lifecycle: Proposed → Reviewed → Approved → Active → Overrun → Closed.
/// </summary>
public static class FinancialsModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "Financials";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "Financials";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "Financials";

    /// <summary>SLA for cost posting: 1 second.</summary>
    public const int CostPostingSlaSeconds = 1;

    /// <summary>SLA for forecast generation: 5 seconds.</summary>
    public const int ForecastGenerationSlaSeconds = 5;

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to create a budget.</summary>
        public const string CreateBudget = "Financials.Budgets.Create";

        /// <summary>Permission to approve a budget.</summary>
        public const string ApproveBudget = "Financials.Budgets.Approve";

        /// <summary>Permission to record a cost.</summary>
        public const string RecordCost = "Financials.Costs.Record";

        /// <summary>Permission to view financial reports.</summary>
        public const string ViewFinancials = "Financials.Reports.View";

        /// <summary>Permission to view ROI analysis.</summary>
        public const string ViewROI = "Financials.ROI.View";
    }
}
