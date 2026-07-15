// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Display-ready view model for a computed Safety ROI
//   calculation, consumed by the ROICard component and ROI dashboard page.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.ROI.Models;

/// <summary>Display-ready view model for a single Safety ROI calculation.</summary>
public sealed class RoiViewModel
{
    /// <summary>Gets the ROI calculation identifier.</summary>
    public required Guid RoiCalculationId { get; init; }

    /// <summary>Gets the fiscal year of the calculation.</summary>
    public required int FiscalYear { get; init; }

    /// <summary>Gets the total safety investment.</summary>
    public required decimal TotalInvestment { get; init; }

    /// <summary>Gets the estimated avoided costs.</summary>
    public required decimal EstimatedAvoidedCosts { get; init; }

    /// <summary>Gets the net benefit (AvoidedCosts - Investment).</summary>
    public required decimal NetBenefit { get; init; }

    /// <summary>Gets the ROI percentage.</summary>
    public required decimal RoiPercentage { get; init; }

    /// <summary>Gets the StatusIndicatorComponent operational state derived from <see cref="RoiPercentage"/> (positive/neutral/negative ROI).</summary>
    public required string StatusIndicatorState { get; init; }

    /// <summary>Gets the ISO 4217 currency code.</summary>
    public required string CurrencyCode { get; init; }

    /// <summary>Gets the UTC timestamp when this ROI was calculated.</summary>
    public required DateTimeOffset CalculatedAtUtc { get; init; }
}
