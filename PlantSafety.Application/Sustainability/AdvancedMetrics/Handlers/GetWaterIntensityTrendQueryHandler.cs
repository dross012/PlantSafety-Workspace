// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           GetWaterIntensityTrendQueryHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handles GetWaterIntensityTrendQuery. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Models;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Queries;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Repositories;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Handlers;

/// <summary>Handles <see cref="GetWaterIntensityTrendQuery"/>.</summary>
public sealed class GetWaterIntensityTrendQueryHandler : IRequestHandler<GetWaterIntensityTrendQuery, Result<IReadOnlyList<WaterIntensityTrendPointDto>>>
{
    private readonly IAdvancedMetricsReadRepository _repository;
    private readonly ILogger<GetWaterIntensityTrendQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetWaterIntensityTrendQueryHandler(IAdvancedMetricsReadRepository repository, ILogger<GetWaterIntensityTrendQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<WaterIntensityTrendPointDto>>> Handle(GetWaterIntensityTrendQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetWaterIntensityTrendAsync(request.AdvancedMetricsLedgerId, request.TenantId, cancellationToken);
            return Result<IReadOnlyList<WaterIntensityTrendPointDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve water intensity trend for Ledger {AdvancedMetricsLedgerId}", request.AdvancedMetricsLedgerId);

            return Result<IReadOnlyList<WaterIntensityTrendPointDto>>.Failure(new ApplicationError(
                "Sustainability.AdvancedMetrics.WaterIntensityTrend.RetrievalFailed",
                "Unable to retrieve water intensity trend data at this time.",
                Guid.NewGuid()));
        }
    }
}
