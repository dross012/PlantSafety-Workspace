// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side DTO for a Safety Return on Investment (ROI) calculation.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Financials.Models.ROI;

/// <summary>
/// Computed Safety ROI for a tenant/site over a fiscal period.
/// Compares safety investment against quantified avoided-cost savings.
/// </summary>
public sealed record SafetyROIDto
{
    /// <summary>Unique identifier of this ROI calculation.</summary>
    public required Guid RoiCalculationId { get; init; }

    /// <summary>Tenant this calculation belongs to.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site this calculation is for. Null means tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Fiscal year of the calculation.</summary>
    public required int FiscalYear { get; init; }

    /// <summary>Total safety investment (sum of budget spend).</summary>
    public required decimal TotalInvestment { get; init; }

    /// <summary>Estimated avoided costs (workers comp savings, incident cost avoidance, regulatory fines avoided).</summary>
    public required decimal EstimatedAvoidedCosts { get; init; }

    /// <summary>Net benefit (AvoidedCosts - Investment).</summary>
    public required decimal NetBenefit { get; init; }

    /// <summary>ROI ratio (NetBenefit / Investment * 100).</summary>
    public required decimal RoiPercentage { get; init; }

    /// <summary>Currency code (ISO 4217).</summary>
    public required string CurrencyCode { get; init; }

    /// <summary>UTC timestamp when this ROI was calculated.</summary>
    public required DateTimeOffset CalculatedAtUtc { get; init; }
}
