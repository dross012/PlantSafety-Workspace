// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Module-scoped constants for the Financials module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Financials.Constants;

/// <summary>Module-scoped constants for Financials.</summary>
public static class FinancialsModuleConstants
{
    /// <summary>Canonical module name.</summary>
    public const string ModuleName = "Financials";

    /// <summary>SQL schema name.</summary>
    public const string SqlSchema = "Financials";

    /// <summary>Service Bus topic name.</summary>
    public const string ServiceBusTopic = "financials";

    /// <summary>Cache key prefix for budget records.</summary>
    public const string BudgetCachePrefix = "budgets";

    /// <summary>Budget cache expiry in minutes.</summary>
    public const int BudgetCacheExpiryMinutes = 10;

    /// <summary>Valid budget statuses.</summary>
    public static readonly IReadOnlyList<string> BudgetStatuses = ["Draft", "Active", "Approved", "Closed"];

    /// <summary>Minimum allowed fiscal year.</summary>
    public const int MinFiscalYear = 2000;

    /// <summary>Maximum allowed fiscal year.</summary>
    public const int MaxFiscalYear = 2100;
}
