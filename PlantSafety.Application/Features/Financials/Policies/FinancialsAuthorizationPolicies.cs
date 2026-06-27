// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Authorization policy names for the Financials module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Financials.Policies;

/// <summary>Authorization policy name constants for Financials features.</summary>
public static class FinancialsAuthorizationPolicies
{
    /// <summary>Policy that grants access to create safety budgets.</summary>
    public const string CanCreateBudget = "Financials.CanCreateBudget";

    /// <summary>Policy that grants access to approve budgets.  Restricted to Finance Managers.</summary>
    public const string CanApproveBudget = "Financials.CanApproveBudget";

    /// <summary>Policy that grants access to record safety costs.</summary>
    public const string CanRecordCost = "Financials.CanRecordCost";

    /// <summary>Policy that grants access to view ROI analysis.</summary>
    public const string CanViewROI = "Financials.CanViewROI";
}
