// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           GetPredictiveMaintenanceForecastQueryHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handles GetPredictiveMaintenanceForecastQuery by re-running the real
//                  FailureLikelihoodPredictor across a series of future time offsets for the
//                  alert's equipment. Additive-only evolution.
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

/// <summary>Handles <see cref="GetPredictiveMaintenanceForecastQuery"/>.</summary>
public sealed class GetPredictiveMaintenanceForecastQueryHandler
    : IRequestHandler<GetPredictiveMaintenanceForecastQuery, Result<PaginatedResult<PredictiveMaintenanceForecastPointDto>>>
{
    private const int ForecastPointCount = 10;
    private static readonly TimeSpan ForecastStep = TimeSpan.FromDays(3);

    private readonly IPredictiveMaintenanceAlertRepository _repository;
    private readonly PredictiveMaintenanceInsightsCalculator _calculator = new();
    private readonly ILogger<GetPredictiveMaintenanceForecastQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPredictiveMaintenanceForecastQueryHandler(
        IPredictiveMaintenanceAlertRepository repository,
        ILogger<GetPredictiveMaintenanceForecastQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<PredictiveMaintenanceForecastPointDto>>> Handle(GetPredictiveMaintenanceForecastQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var alert = await _repository.GetByIdAsync(request.PredictionId, request.TenantId, cancellationToken);
            if (alert is null)
            {
                return Result<PaginatedResult<PredictiveMaintenanceForecastPointDto>>.Failure(new ApplicationError(
                    "Operational.PredictiveMaintenance.Forecast.NotFound",
                    "The requested prediction could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            var now = DateTime.UtcNow;
            var points = new List<PredictiveMaintenanceForecastPointDto>(ForecastPointCount);

            for (var i = 0; i < ForecastPointCount; i++)
            {
                var forecastAsOf = now + (ForecastStep * i);
                var prediction = _calculator.PredictForAlert(alert.Status, alert.PredictedAtUtc, forecastAsOf);
                var band = (1.0 - prediction.Confidence.Value) * 0.15;

                points.Add(new PredictiveMaintenanceForecastPointDto
                {
                    TimestampUtc = forecastAsOf,
                    RiskScore = prediction.Output.FailureLikelihood,
                    ConfidenceLower = Math.Round(Math.Max(0.0, prediction.Output.FailureLikelihood - band), 4),
                    ConfidenceUpper = Math.Round(Math.Min(1.0, prediction.Output.FailureLikelihood + band), 4),
                });
            }

            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = Math.Max(1, request.PageSize);
            var page = points.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Result<PaginatedResult<PredictiveMaintenanceForecastPointDto>>.Success(
                new PaginatedResult<PredictiveMaintenanceForecastPointDto>(page, points.Count, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute forecast for PredictiveMaintenanceAlert {PredictionId}", request.PredictionId);

            return Result<PaginatedResult<PredictiveMaintenanceForecastPointDto>>.Failure(new ApplicationError(
                "Operational.PredictiveMaintenance.Forecast.RetrievalFailed",
                "Unable to retrieve the risk forecast at this time.",
                Guid.NewGuid()));
        }
    }
}
