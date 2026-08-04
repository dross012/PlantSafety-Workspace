// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           AcknowledgeCognitiveRiskAlertCommandHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for AcknowledgeCognitiveRiskAlertCommand. Loads the target
//                  CognitiveRiskPrediction by identity and acknowledges it via the domain
//                  aggregate method. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Interfaces;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;

/// <summary>Handles <see cref="AcknowledgeCognitiveRiskAlertCommand"/>.</summary>
public sealed class AcknowledgeCognitiveRiskAlertCommandHandler : IRequestHandler<AcknowledgeCognitiveRiskAlertCommand, Result<AcknowledgeCognitiveRiskAlertResult>>
{
    private readonly ICognitiveRiskPredictionRepository _repository;
    private readonly ILogger<AcknowledgeCognitiveRiskAlertCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AcknowledgeCognitiveRiskAlertCommandHandler(ICognitiveRiskPredictionRepository repository, ILogger<AcknowledgeCognitiveRiskAlertCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AcknowledgeCognitiveRiskAlertResult>> Handle(AcknowledgeCognitiveRiskAlertCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var prediction = await _repository.GetByIdAsync(CognitiveRiskPredictionId.FromGuid(request.CognitiveRiskPredictionId), request.TenantId, cancellationToken);
            if (prediction is null)
            {
                return Result<AcknowledgeCognitiveRiskAlertResult>.Failure(new ApplicationError(
                    "HumanFactors.CognitiveLoadAnalytics.CognitiveRiskPrediction.NotFound",
                    $"CognitiveRiskPrediction {request.CognitiveRiskPredictionId} was not found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var requesterId = UserId.FromGuid(request.RequesterUserId);
            var acknowledgedAtUtc = UtcTimestamp.Now();

            prediction.Acknowledge(
                acknowledgedBy: requesterId,
                notes: request.AcknowledgementNotes,
                atUtc: acknowledgedAtUtc,
                actor: PerformedByActor.User(requesterId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(prediction, cancellationToken);

            return Result<AcknowledgeCognitiveRiskAlertResult>.Success(new AcknowledgeCognitiveRiskAlertResult(
                prediction.CognitiveRiskPredictionId.Value, prediction.Status.ToString(), acknowledgedAtUtc.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acknowledge cognitive risk alert {CognitiveRiskPredictionId}", request.CognitiveRiskPredictionId);

            return Result<AcknowledgeCognitiveRiskAlertResult>.Failure(new ApplicationError(
                "HumanFactors.CognitiveLoadAnalytics.AcknowledgeCognitiveRiskAlertFailed",
                "Unable to acknowledge the cognitive risk alert at this time.",
                request.CorrelationId));
        }
    }
}
