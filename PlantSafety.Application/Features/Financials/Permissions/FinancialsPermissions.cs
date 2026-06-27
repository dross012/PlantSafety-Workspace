// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Permission constants for the Financials module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Financials.Permissions;

/// <summary>Permission strings for the Financials module.  Format: <c>Module.Feature.Action</c>.</summary>
public static class FinancialsPermissions
{
    /// <summary>Budget permissions.</summary>
    public static class Budgets
    {
        /// <summary>Create a safety budget.</summary>
        public const string Create = "Financials.Budgets.Create";
        /// <summary>Approve a budget.</summary>
        public const string Approve = "Financials.Budgets.Approve";
        /// <summary>View a budget.</summary>
        public const string View = "Financials.Budgets.View";
        /// <summary>List budgets.</summary>
        public const string List = "Financials.Budgets.List";
    }

    /// <summary>Cost recording permissions.</summary>
    public static class Costs
    {
        /// <summary>Record a safety cost entry.</summary>
        public const string Record = "Financials.Costs.Record";
        /// <summary>View cost records.</summary>
        public const string View = "Financials.Costs.View";
        /// <summary>List cost records.</summary>
        public const string List = "Financials.Costs.List";
    }

    /// <summary>ROI analysis permissions.</summary>
    public static class ROI
    {
        /// <summary>View safety ROI analysis.</summary>
        public const string View = "Financials.ROI.View";
    }
}
