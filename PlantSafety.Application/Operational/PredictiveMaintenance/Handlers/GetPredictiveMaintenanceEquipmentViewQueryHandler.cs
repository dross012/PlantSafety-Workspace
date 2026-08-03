// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           GetPredictiveMaintenanceEquipmentViewQueryHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handles GetPredictiveMaintenanceEquipmentViewQuery. Additive-only evolution.
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

/// <summary>Handles <see cref="GetPredictiveMaintenanceEquipmentViewQuery"/>.</summary>
public sealed class GetPredictiveMaintenanceEquipmentViewQueryHandler
    : IRequestHandler<GetPredictiveMaintenanceEquipmentViewQuery, Result<PredictiveMaintenanceEquipmentViewDto>>
{
    private readonly IPredictiveMaintenanceAlertRepository _repository;
    private readonly PredictiveMaintenanceInsightsCalculator _calculator = new();
    private readonly ILogger<GetPredictiveMaintenanceEquipmentViewQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPredictiveMaintenanceEquipmentViewQueryHandler(
        IPredictiveMaintenanceAlertRepository repository,
        ILogger<GetPredictiveMaintenanceEquipmentViewQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PredictiveMaintenanceEquipmentViewDto>> Handle(GetPredictiveMaintenanceEquipmentViewQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (items, _) = await _repository.GetPagedAsync(
                request.TenantId, request.EquipmentId, activeOnly: false, pageNumber: 1, pageSize: 1, cancellationToken);

            var latest = items.FirstOrDefault();

            if (latest is null)
            {
                return Result<PredictiveMaintenanceEquipmentViewDto>.Failure(new ApplicationError(
                    "Operational.PredictiveMaintenance.EquipmentView.NotFound",
                    "No predictive maintenance history was found for the requested equipment.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            var bucket = PredictiveMaintenanceInsightsCalculator.ClassifyStatus(latest.Status);
            var prediction = _calculator.PredictForAlert(latest.Status, latest.PredictedAtUtc, DateTime.UtcNow);
            var equipmentCode = PredictiveMaintenanceInsightsCalculator.ShortCode("EQ", request.EquipmentId);

            return Result<PredictiveMaintenanceEquipmentViewDto>.Success(new PredictiveMaintenanceEquipmentViewDto
            {
                EquipmentId = request.EquipmentId,
                EquipmentCode = equipmentCode,
                EquipmentName = equipmentCode,
                HealthStatus = PredictiveMaintenanceInsightsCalculator.HealthLabel(bucket),
                RiskScore = prediction.Output.FailureLikelihood,
                LatestPredictionId = bucket == PredictiveMaintenanceRiskBucket.Low ? null : latest.PredictiveMaintenanceAlertId,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PredictiveMaintenance equipment view for Tenant {TenantId} Equipment {EquipmentId}", request.TenantId, request.EquipmentId);

            return Result<PredictiveMaintenanceEquipmentViewDto>.Failure(new ApplicationError(
                "Operational.PredictiveMaintenance.EquipmentView.RetrievalFailed",
                "Unable to retrieve the predictive equipment view at this time.",
                Guid.NewGuid()));
        }
    }
}
