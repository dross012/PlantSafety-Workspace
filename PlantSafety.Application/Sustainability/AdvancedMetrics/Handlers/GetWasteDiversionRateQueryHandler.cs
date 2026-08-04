// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           GetWasteDiversionRateQueryHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handles GetWasteDiversionRateQuery. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Models;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Queries;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Repositories;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Handlers;

/// <summary>Handles <see cref="GetWasteDiversionRateQuery"/>.</summary>
public sealed class GetWasteDiversionRateQueryHandler : IRequestHandler<GetWasteDiversionRateQuery, Result<WasteDiversionRateDto>>
{
    private readonly IAdvancedMetricsReadRepository _repository;
    private readonly ILogger<GetWasteDiversionRateQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetWasteDiversionRateQueryHandler(IAdvancedMetricsReadRepository repository, ILogger<GetWasteDiversionRateQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WasteDiversionRateDto>> Handle(GetWasteDiversionRateQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetWasteDiversionRateAsync(request.AdvancedMetricsLedgerId, request.TenantId, cancellationToken);

            if (item is null)
            {
                return Result<WasteDiversionRateDto>.Failure(new ApplicationError(
                    "Sustainability.AdvancedMetrics.WasteDiversionRate.NotFound",
                    "No waste diversion data was found for the requested ledger.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<WasteDiversionRateDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve waste diversion rate for Ledger {AdvancedMetricsLedgerId}", request.AdvancedMetricsLedgerId);

            return Result<WasteDiversionRateDto>.Failure(new ApplicationError(
                "Sustainability.AdvancedMetrics.WasteDiversionRate.RetrievalFailed",
                "Unable to retrieve waste diversion rate data at this time.",
                Guid.NewGuid()));
        }
    }
}
