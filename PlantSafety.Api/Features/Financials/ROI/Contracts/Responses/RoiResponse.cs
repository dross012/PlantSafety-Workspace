// ============================================================================
// File: RoiResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: External-facing response DTOs for the ROI module. Field names/shape
//              mirror PlantSafety.Application.Features.Financials.Models.ROI.SafetyROIDto
//              exactly, matching the SafetyRoiDto already coded into PlantSafety.UI.
// ============================================================================

namespace PlantSafety.Api.Features.Financials.ROI.Contracts.Responses;

/// <summary>Response payload for a computed Safety ROI calculation.</summary>
public sealed record RoiResponse(
    Guid           RoiCalculationId,
    Guid           TenantId,
    Guid?          SiteId,
    int            FiscalYear,
    decimal        TotalInvestment,
    decimal        EstimatedAvoidedCosts,
    decimal        NetBenefit,
    decimal        RoiPercentage,
    string         CurrencyCode,
    DateTimeOffset CalculatedAtUtc);

/// <summary>Paginated response payload for ROI calculation history.</summary>
public sealed record RoiHistoryListResponse(
    IReadOnlyList<RoiResponse> Items,
    int                        TotalCount,
    int                        PageNumber,
    int                        PageSize,
    int                        TotalPages,
    bool                       HasNextPage,
    bool                       HasPreviousPage);

/// <summary>Response payload returned after recording a Safety ROI calculation.</summary>
/// <param name="RoiCalculationId">Identifier of the newly recorded ROI calculation.</param>
public sealed record CalculateRoiResponse(Guid RoiCalculationId);
