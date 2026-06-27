// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Validation constants specific to the Financials module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Financials.Validation;

/// <summary>Validation constraint constants for Financials features.</summary>
public static class FinancialsValidationConstants
{
    /// <summary>Maximum budget title length.</summary>
    public const int MaxTitleLength = 200;

    /// <summary>Maximum budget description length.</summary>
    public const int MaxDescriptionLength = 4_000;

    /// <summary>Minimum allowed monetary amount (exclusive).</summary>
    public const decimal MinAmount = 0m;

    /// <summary>Maximum allowed monetary amount for a single budget.</summary>
    public const decimal MaxBudgetAmount = 999_999_999.99m;

    /// <summary>ISO 4217 currency code validation regex pattern.</summary>
    public const string CurrencyCodePattern = "^[A-Z]{3}$";

    /// <summary>Minimum allowed fiscal year.</summary>
    public const int MinFiscalYear = 2000;

    /// <summary>Maximum allowed fiscal year.</summary>
    public const int MaxFiscalYear = 2100;
}
