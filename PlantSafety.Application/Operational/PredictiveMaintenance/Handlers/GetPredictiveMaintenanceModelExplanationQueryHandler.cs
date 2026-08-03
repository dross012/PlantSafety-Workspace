// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           GetPredictiveMaintenanceModelExplanationQueryHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handles GetPredictiveMaintenanceModelExplanationQuery. Additive-only evolution.
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

/// <summary>Handles <see cref="GetPredictiveMaintenanceModelExplanationQuery"/>.</summary>
public sealed class GetPredictiveMaintenanceModelExplanationQueryHandler
    : IRequestHandler<GetPredictiveMaintenanceModelExplanationQuery, Result<PredictiveMaintenanceModelExplanationDto>>
{
    private readonly IPredictiveMaintenanceAlertRepository _repository;
    private readonly PredictiveMaintenanceInsightsCalculator _calculator = new();
    private readonly ILogger<GetPredictiveMaintenanceModelExplanationQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPredictiveMaintenanceModelExplanationQueryHandler(
        IPredictiveMaintenanceAlertRepository repository,
        ILogger<GetPredictiveMaintenanceModelExplanationQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PredictiveMaintenanceModelExplanationDto>> Handle(GetPredictiveMaintenanceModelExplanationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var alert = await _repository.GetByIdAsync(request.PredictionId, request.TenantId, cancellationToken);
            if (alert is null)
            {
                return Result<PredictiveMaintenanceModelExplanationDto>.Failure(new ApplicationError(
                    "Operational.PredictiveMaintenance.ModelExplanation.NotFound",
                    "The requested prediction could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            var prediction = _calculator.PredictForAlert(alert.Status, alert.PredictedAtUtc, DateTime.UtcNow);

            return Result<PredictiveMaintenanceModelExplanationDto>.Success(new PredictiveMaintenanceModelExplanationDto
            {
                Factors = PredictiveMaintenanceInsightsCalculator.BuildFeatureImportance(),
                ModelVersion = prediction.ModelVersion,
                ExplanationSummary = prediction.Rationale ?? string.Empty,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute model explanation for PredictiveMaintenanceAlert {PredictionId}", request.PredictionId);

            return Result<PredictiveMaintenanceModelExplanationDto>.Failure(new ApplicationError(
                "Operational.PredictiveMaintenance.ModelExplanation.RetrievalFailed",
                "Unable to retrieve the model explanation at this time.",
                Guid.NewGuid()));
        }
    }
}
