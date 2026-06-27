// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Abstractions for the Financials module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Financials.Abstractions;

/// <summary>Provides financial reporting and ROI calculation services.</summary>
public interface IFinancialReportingService
{
    /// <summary>Returns the computed safety ROI for the given tenant and fiscal year.</summary>
    Task<RoiResult?> ComputeRoiAsync(
        Guid tenantId,
        Guid? siteId,
        int fiscalYear,
        CancellationToken cancellationToken);
}

/// <summary>ROI calculation result.</summary>
public sealed record RoiResult
{
    /// <summary>Total investment in safety programs.</summary>
    public required decimal TotalInvestment { get; init; }
    /// <summary>Estimated avoided costs.</summary>
    public required decimal EstimatedAvoidedCosts { get; init; }
    /// <summary>Net benefit (AvoidedCosts - TotalInvestment).</summary>
    public required decimal NetBenefit { get; init; }
    /// <summary>ROI percentage ((NetBenefit / TotalInvestment) × 100).</summary>
    public required decimal RoiPercentage { get; init; }
    /// <summary>ISO 4217 currency code.</summary>
    public required string CurrencyCode { get; init; }
}

/// <summary>Validates that a budget's currency code is supported for the tenant's locale.</summary>
public interface IBudgetCurrencyValidator
{
    /// <summary>Returns <c>true</c> when the currency code is supported.</summary>
    Task<bool> IsSupportedAsync(Guid tenantId, string currencyCode, CancellationToken cancellationToken);
}
