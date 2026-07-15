// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTO mirroring the Application layer's
//   SafetyROIDto (PlantSafety.Application.Features.Financials.Models.ROI).
//   No corresponding HTTP controller currently exists in PlantSafety.Api
//   (only the Budgets sub-tree of Features/Financials is wired to a
//   controller) — GET requests against the plausible route below
//   (v1/features/financials/roi) will resolve to a Problem/Fault envelope
//   via SafetyApiClientBase's existing handling, and the ROI dashboard page
//   renders a "needs backend wiring" notice in that case rather than
//   blocking, per the UI Agent's degrade-gracefully mandate.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.ROI.Models;

/// <summary>Computed Safety ROI for a tenant/site over a fiscal period.</summary>
public sealed class SafetyRoiDto
{
    /// <summary>Unique identifier of this ROI calculation.</summary>
    public Guid RoiCalculationId { get; init; }

    /// <summary>Tenant this calculation belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site this calculation is for. Null means tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Fiscal year of the calculation.</summary>
    public int FiscalYear { get; init; }

    /// <summary>Total safety investment (sum of budget spend).</summary>
    public decimal TotalInvestment { get; init; }

    /// <summary>Estimated avoided costs (workers comp savings, incident cost avoidance, regulatory fines avoided).</summary>
    public decimal EstimatedAvoidedCosts { get; init; }

    /// <summary>Net benefit (AvoidedCosts - Investment).</summary>
    public decimal NetBenefit { get; init; }

    /// <summary>ROI ratio (NetBenefit / Investment * 100).</summary>
    public decimal RoiPercentage { get; init; }

    /// <summary>Currency code (ISO 4217).</summary>
    public string CurrencyCode { get; init; } = string.Empty;

    /// <summary>UTC timestamp when this ROI was calculated.</summary>
    public DateTimeOffset CalculatedAtUtc { get; init; }
}
