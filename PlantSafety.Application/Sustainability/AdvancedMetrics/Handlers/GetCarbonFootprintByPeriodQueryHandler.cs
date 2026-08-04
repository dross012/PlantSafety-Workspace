// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           GetCarbonFootprintByPeriodQueryHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handles GetCarbonFootprintByPeriodQuery. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Models;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Queries;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Repositories;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Handlers;

/// <summary>Handles <see cref="GetCarbonFootprintByPeriodQuery"/>.</summary>
public sealed class GetCarbonFootprintByPeriodQueryHandler : IRequestHandler<GetCarbonFootprintByPeriodQuery, Result<CarbonFootprintByPeriodDto>>
{
    private readonly IAdvancedMetricsReadRepository _repository;
    private readonly ILogger<GetCarbonFootprintByPeriodQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCarbonFootprintByPeriodQueryHandler(IAdvancedMetricsReadRepository repository, ILogger<GetCarbonFootprintByPeriodQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CarbonFootprintByPeriodDto>> Handle(GetCarbonFootprintByPeriodQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetCarbonFootprintByPeriodAsync(
                request.TenantId, request.SiteId, request.PeriodStartUtc, request.PeriodEndUtc, cancellationToken);

            if (item is null)
            {
                return Result<CarbonFootprintByPeriodDto>.Failure(new ApplicationError(
                    "Sustainability.AdvancedMetrics.CarbonFootprint.NotFound",
                    "No carbon footprint data was found for the requested period.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<CarbonFootprintByPeriodDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve carbon footprint for Tenant {TenantId}", request.TenantId);

            return Result<CarbonFootprintByPeriodDto>.Failure(new ApplicationError(
                "Sustainability.AdvancedMetrics.CarbonFootprint.RetrievalFailed",
                "Unable to retrieve carbon footprint data at this time.",
                Guid.NewGuid()));
        }
    }
}
