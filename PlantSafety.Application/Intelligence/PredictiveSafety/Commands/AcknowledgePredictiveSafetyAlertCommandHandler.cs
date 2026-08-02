// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.PredictiveSafety
//  Layer:          Application
//  File:           AcknowledgePredictiveSafetyAlertCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for AcknowledgePredictiveSafetyAlertCommand. GetByIdAsync is
//                  tenant-scoped, the NotFound ApplicationError uses ErrorSeverity.NotFound, and
//                  ConcurrencyConflictException is re-thrown before the generic catch.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Intelligence.PredictiveSafety.Interfaces;
using PlantSafety.Domain.Intelligence.PredictiveSafety.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Intelligence.PredictiveSafety.Commands;

/// <summary>Handles <see cref="AcknowledgePredictiveSafetyAlertCommand"/>.</summary>
public sealed class AcknowledgePredictiveSafetyAlertCommandHandler : IRequestHandler<AcknowledgePredictiveSafetyAlertCommand, Result<PredictiveSafetyAlertStatusResult>>
{
    private readonly IPredictiveSafetyAlertRepository _repository;
    private readonly ILogger<AcknowledgePredictiveSafetyAlertCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AcknowledgePredictiveSafetyAlertCommandHandler(IPredictiveSafetyAlertRepository repository, ILogger<AcknowledgePredictiveSafetyAlertCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PredictiveSafetyAlertStatusResult>> Handle(AcknowledgePredictiveSafetyAlertCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PredictiveSafetyAlertId.FromGuid(request.PredictiveSafetyAlertId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<PredictiveSafetyAlertStatusResult>.Failure(new ApplicationError(
                    "Intelligence.PredictiveSafety.NotFound",
                    "The requested predictive safety alert could not be found.",
                    request.PredictiveSafetyAlertId,
                    ErrorSeverity.NotFound));
            }

            var acknowledgedBy = UserId.FromGuid(request.AcknowledgedByUserId);

            aggregate.Acknowledge(
                acknowledgedBy: acknowledgedBy,
                acknowledgedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(acknowledgedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<PredictiveSafetyAlertStatusResult>.Success(new PredictiveSafetyAlertStatusResult(
                aggregate.PredictiveSafetyAlertId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acknowledge PredictiveSafetyAlert {PredictiveSafetyAlertId}", request.PredictiveSafetyAlertId);

            return Result<PredictiveSafetyAlertStatusResult>.Failure(new ApplicationError(
                "Intelligence.PredictiveSafety.AcknowledgeFailed",
                "Unable to acknowledge the predictive safety alert at this time.",
                Guid.NewGuid()));
        }
    }
}
