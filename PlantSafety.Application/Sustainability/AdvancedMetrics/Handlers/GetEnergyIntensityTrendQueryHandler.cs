// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           GetEnergyIntensityTrendQueryHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handles GetEnergyIntensityTrendQuery. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Models;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Queries;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Repositories;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Handlers;

/// <summary>Handles <see cref="GetEnergyIntensityTrendQuery"/>.</summary>
public sealed class GetEnergyIntensityTrendQueryHandler : IRequestHandler<GetEnergyIntensityTrendQuery, Result<IReadOnlyList<EnergyIntensityTrendPointDto>>>
{
    private readonly IAdvancedMetricsReadRepository _repository;
    private readonly ILogger<GetEnergyIntensityTrendQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEnergyIntensityTrendQueryHandler(IAdvancedMetricsReadRepository repository, ILogger<GetEnergyIntensityTrendQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<EnergyIntensityTrendPointDto>>> Handle(GetEnergyIntensityTrendQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetEnergyIntensityTrendAsync(request.AdvancedMetricsLedgerId, request.TenantId, cancellationToken);
            return Result<IReadOnlyList<EnergyIntensityTrendPointDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve energy intensity trend for Ledger {AdvancedMetricsLedgerId}", request.AdvancedMetricsLedgerId);

            return Result<IReadOnlyList<EnergyIntensityTrendPointDto>>.Failure(new ApplicationError(
                "Sustainability.AdvancedMetrics.EnergyIntensityTrend.RetrievalFailed",
                "Unable to retrieve energy intensity trend data at this time.",
                Guid.NewGuid()));
        }
    }
}
