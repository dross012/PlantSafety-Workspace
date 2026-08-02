// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Application
//  File:           CreatePredictiveAnalyticsCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreatePredictiveAnalyticsCommand. Pure orchestration: constructs
//                  the PredictiveAnalytics aggregate via its domain factory method and persists it
//                  through the domain repository interface. No business rules live here. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Analytics.PredictiveAnalytics.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.PredictiveAnalytics.Commands;

/// <summary>Handles <see cref="CreatePredictiveAnalyticsCommand"/>.</summary>
public sealed class CreatePredictiveAnalyticsCommandHandler : IRequestHandler<CreatePredictiveAnalyticsCommand, Result<CreatePredictiveAnalyticsResult>>
{
    private readonly IPredictiveAnalyticsRepository _repository;
    private readonly ILogger<CreatePredictiveAnalyticsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreatePredictiveAnalyticsCommandHandler(IPredictiveAnalyticsRepository repository, ILogger<CreatePredictiveAnalyticsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreatePredictiveAnalyticsResult>> Handle(CreatePredictiveAnalyticsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);

            var aggregate = PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                modelName: request.ModelName,
                predictionType: request.PredictionType,
                targetEntityId: request.TargetEntityId,
                predictedRiskScore: request.PredictedRiskScore,
                confidenceLevel: request.ConfidenceLevel,
                predictionHorizonDays: request.PredictionHorizonDays);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreatePredictiveAnalyticsResult>.Success(new CreatePredictiveAnalyticsResult(
                aggregate.PredictiveAnalyticsId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create PredictiveAnalytics for Tenant {TenantId}", request.TenantId);

            return Result<CreatePredictiveAnalyticsResult>.Failure(new ApplicationError(
                "Analytics.PredictiveAnalytics.CreateFailed",
                "Unable to submit the predictive analytics record at this time.",
                Guid.NewGuid()));
        }
    }
}
