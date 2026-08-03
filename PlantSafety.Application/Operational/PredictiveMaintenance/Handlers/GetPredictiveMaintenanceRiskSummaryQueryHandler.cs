// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           GetPredictiveMaintenanceRiskSummaryQueryHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handles GetPredictiveMaintenanceRiskSummaryQuery by aggregating real
//                  PredictiveMaintenanceAlert records into risk buckets. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Constants;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;
using PlantSafety.Application.Operational.PredictiveMaintenance.Queries;
using PlantSafety.Application.Operational.PredictiveMaintenance.Repositories;
using PlantSafety.Application.Operational.PredictiveMaintenance.Services;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Handlers;

/// <summary>Handles <see cref="GetPredictiveMaintenanceRiskSummaryQuery"/>.</summary>
public sealed class GetPredictiveMaintenanceRiskSummaryQueryHandler
    : IRequestHandler<GetPredictiveMaintenanceRiskSummaryQuery, Result<PredictiveMaintenanceRiskSummaryDto>>
{
    private readonly IPredictiveMaintenanceAlertRepository _repository;
    private readonly ILogger<GetPredictiveMaintenanceRiskSummaryQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPredictiveMaintenanceRiskSummaryQueryHandler(
        IPredictiveMaintenanceAlertRepository repository,
        ILogger<GetPredictiveMaintenanceRiskSummaryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PredictiveMaintenanceRiskSummaryDto>> Handle(GetPredictiveMaintenanceRiskSummaryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (items, _) = await _repository.GetPagedAsync(
                request.TenantId, equipmentId: null, activeOnly: false, pageNumber: 1, pageSize: PaginationConstants.MaxPageSize, cancellationToken);

            var high = 0;
            var medium = 0;
            var low = 0;

            var now = DateTime.UtcNow;
            var currentWindowCount = 0;
            var previousWindowCount = 0;

            foreach (var item in items)
            {
                switch (PredictiveMaintenanceInsightsCalculator.ClassifyStatus(item.Status))
                {
                    case PredictiveMaintenanceRiskBucket.High: high++; break;
                    case PredictiveMaintenanceRiskBucket.Medium: medium++; break;
                    default: low++; break;
                }

                var age = now - item.PredictedAtUtc;
                if (age <= TimeSpan.FromDays(7)) currentWindowCount++;
                else if (age <= TimeSpan.FromDays(14)) previousWindowCount++;
            }

            var trend = currentWindowCount > previousWindowCount ? "Increasing"
                : currentWindowCount < previousWindowCount ? "Decreasing"
                : "Stable";

            return Result<PredictiveMaintenanceRiskSummaryDto>.Success(new PredictiveMaintenanceRiskSummaryDto
            {
                HighRiskCount = high,
                MediumRiskCount = medium,
                LowRiskCount = low,
                TrendDirection = trend,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute PredictiveMaintenance risk summary for Tenant {TenantId}", request.TenantId);

            return Result<PredictiveMaintenanceRiskSummaryDto>.Failure(new ApplicationError(
                "Operational.PredictiveMaintenance.RiskSummary.RetrievalFailed",
                "Unable to retrieve the predictive maintenance risk summary at this time.",
                Guid.NewGuid()));
        }
    }
}
