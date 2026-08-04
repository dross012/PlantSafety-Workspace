// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           GenerateCognitiveRiskPredictionCommandHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for GenerateCognitiveRiskPredictionCommand. Loads the source
//                  CognitiveLoadLedger, evaluates predicted risk via CognitiveLoadAnalyticsService
//                  (mean load / overload frequency -> CognitiveRiskPredictionPolicy), then
//                  generates and persists the CognitiveRiskPrediction aggregate.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Interfaces;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Services;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;

/// <summary>Handles <see cref="GenerateCognitiveRiskPredictionCommand"/>.</summary>
public sealed class GenerateCognitiveRiskPredictionCommandHandler : IRequestHandler<GenerateCognitiveRiskPredictionCommand, Result<GenerateCognitiveRiskPredictionResult>>
{
    private readonly ICognitiveLoadLedgerRepository _ledgerRepository;
    private readonly ICognitiveRiskPredictionRepository _predictionRepository;
    private readonly CognitiveLoadAnalyticsService _analyticsService;
    private readonly ILogger<GenerateCognitiveRiskPredictionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GenerateCognitiveRiskPredictionCommandHandler(
        ICognitiveLoadLedgerRepository ledgerRepository,
        ICognitiveRiskPredictionRepository predictionRepository,
        CognitiveLoadAnalyticsService analyticsService,
        ILogger<GenerateCognitiveRiskPredictionCommandHandler> logger)
    {
        _ledgerRepository = ledgerRepository;
        _predictionRepository = predictionRepository;
        _analyticsService = analyticsService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GenerateCognitiveRiskPredictionResult>> Handle(GenerateCognitiveRiskPredictionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var ledger = await _ledgerRepository.GetByIdAsync(CognitiveLoadLedgerId.FromGuid(request.SourceLedgerId), request.TenantId, cancellationToken);
            if (ledger is null)
            {
                return Result<GenerateCognitiveRiskPredictionResult>.Failure(new ApplicationError(
                    "HumanFactors.CognitiveLoadAnalytics.CognitiveLoadLedger.NotFound",
                    $"CognitiveLoadLedger {request.SourceLedgerId} was not found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var meanLoad = _analyticsService.ComputeMeanCognitiveLoad(ledger) ?? 0m;
            var overloadFrequency = _analyticsService.ComputeOverloadEventFrequencyPercent(ledger);
            var riskLevel = _analyticsService.EvaluatePredictedRiskLevel(ledger);
            var predictionScore = _analyticsService.EstimatePredictionScore(ledger);

            var requesterId = UserId.FromGuid(request.RequesterUserId);
            var basis = $"MeanCognitiveLoadScore={meanLoad:N2}; OverloadEventFrequencyPercent={overloadFrequency:N2}; ReadingCount={ledger.CognitiveLoadReadings.Count}";

            var prediction = PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Aggregates.CognitiveRiskPrediction.Generate(
                generatedBy: requesterId,
                generatedAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                workerUserId: UserId.FromGuid(request.WorkerUserId),
                sourceLedgerId: ledger.CognitiveLoadLedgerId,
                predictedRiskLevel: riskLevel,
                predictionScore: predictionScore,
                modelVersion: request.ModelVersion,
                predictionBasis: basis);

            await _predictionRepository.AddAsync(prediction, cancellationToken);

            return Result<GenerateCognitiveRiskPredictionResult>.Success(new GenerateCognitiveRiskPredictionResult(
                prediction.CognitiveRiskPredictionId.Value, prediction.PredictedRiskLevel.ToString(),
                prediction.PredictionScore, prediction.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate cognitive risk prediction for Tenant {TenantId}", request.TenantId);

            return Result<GenerateCognitiveRiskPredictionResult>.Failure(new ApplicationError(
                "HumanFactors.CognitiveLoadAnalytics.GenerateCognitiveRiskPredictionFailed",
                "Unable to generate the cognitive risk prediction at this time.",
                request.CorrelationId));
        }
    }
}
