// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           GetPredictiveMaintenancePredictionsQueryHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handles GetPredictiveMaintenancePredictionsQuery. Maps real
//                  PredictiveMaintenanceAlert rows into prediction rows, computing each row's
//                  confidence via a real FailureLikelihoodPredictor invocation. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;
using PlantSafety.Application.Operational.PredictiveMaintenance.Queries;
using PlantSafety.Application.Operational.PredictiveMaintenance.Repositories;
using PlantSafety.Application.Operational.PredictiveMaintenance.Services;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Handlers;

/// <summary>Handles <see cref="GetPredictiveMaintenancePredictionsQuery"/>.</summary>
public sealed class GetPredictiveMaintenancePredictionsQueryHandler
    : IRequestHandler<GetPredictiveMaintenancePredictionsQuery, Result<PaginatedResult<PredictiveMaintenancePredictionRowDto>>>
{
    private readonly IPredictiveMaintenanceAlertRepository _repository;
    private readonly PredictiveMaintenanceInsightsCalculator _calculator = new();
    private readonly ILogger<GetPredictiveMaintenancePredictionsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPredictiveMaintenancePredictionsQueryHandler(
        IPredictiveMaintenanceAlertRepository repository,
        ILogger<GetPredictiveMaintenancePredictionsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<PredictiveMaintenancePredictionRowDto>>> Handle(GetPredictiveMaintenancePredictionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = Math.Max(1, request.PageSize);

            var (items, totalCount) = await _repository.GetPagedAsync(
                request.TenantId, equipmentId: null, activeOnly: request.ActiveOnly, pageNumber, pageSize, cancellationToken);

            var now = DateTime.UtcNow;

            var rows = items.Select(item =>
            {
                var bucket = PredictiveMaintenanceInsightsCalculator.ClassifyStatus(item.Status);
                var prediction = _calculator.PredictForAlert(item.Status, item.PredictedAtUtc, now);
                var updatedAt = item.ClosedAtUtc ?? item.DismissedAtUtc ?? item.ResolvedAtUtc
                    ?? item.ActionScheduledAtUtc ?? item.ReviewedAtUtc ?? item.PredictedAtUtc;

                return new PredictiveMaintenancePredictionRowDto
                {
                    PredictionId = item.PredictiveMaintenanceAlertId,
                    PredictionCode = PredictiveMaintenanceInsightsCalculator.ShortCode("P", item.PredictiveMaintenanceAlertId),
                    EquipmentId = item.EquipmentId,
                    EquipmentCode = PredictiveMaintenanceInsightsCalculator.ShortCode("EQ", item.EquipmentId),
                    RiskLevel = PredictiveMaintenanceInsightsCalculator.BucketLabel(bucket),
                    ConfidencePercent = Math.Round(prediction.Confidence.Value * 100.0, 1),
                    UpdatedAtUtc = updatedAt,
                    IsActive = bucket != PredictiveMaintenanceRiskBucket.Low,
                };
            }).ToList();

            return Result<PaginatedResult<PredictiveMaintenancePredictionRowDto>>.Success(
                new PaginatedResult<PredictiveMaintenancePredictionRowDto>(rows, totalCount, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PredictiveMaintenance predictions for Tenant {TenantId}", request.TenantId);

            return Result<PaginatedResult<PredictiveMaintenancePredictionRowDto>>.Failure(new ApplicationError(
                "Operational.PredictiveMaintenance.Predictions.RetrievalFailed",
                "Unable to retrieve predictive maintenance predictions at this time.",
                Guid.NewGuid()));
        }
    }
}
