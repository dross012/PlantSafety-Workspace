// ============================================================================
// File: CalculateRoiRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing request DTO for POST v1/features/financials/roi,
//              bound from the HTTP request body.
// ============================================================================

namespace PlantSafety.Api.Features.Financials.ROI.Contracts.Requests;

/// <summary>Request body for recording a new Safety ROI calculation.</summary>
public sealed record CalculateRoiRequest(
    Guid?   SiteId,
    int     FiscalYear,
    decimal TotalInvestment,
    decimal EstimatedAvoidedCosts,
    string  CurrencyCode);
