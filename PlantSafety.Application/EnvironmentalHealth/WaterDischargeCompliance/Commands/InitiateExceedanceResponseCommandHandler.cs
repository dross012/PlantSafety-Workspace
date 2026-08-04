// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for InitiateExceedanceResponseCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Interfaces;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;

/// <summary>Handles <see cref="InitiateExceedanceResponseCommand"/>.</summary>
public sealed class InitiateExceedanceResponseCommandHandler : IRequestHandler<InitiateExceedanceResponseCommand, Result<LimitExceedanceActionResult>>
{
    private readonly ILimitExceedanceRepository _repository;
    private readonly ILogger<InitiateExceedanceResponseCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public InitiateExceedanceResponseCommandHandler(ILimitExceedanceRepository repository, ILogger<InitiateExceedanceResponseCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LimitExceedanceActionResult>> Handle(InitiateExceedanceResponseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = LimitExceedanceId.FromGuid(request.LimitExceedanceId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<LimitExceedanceActionResult>.Failure(new ApplicationError(
                    "EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.NotFound",
                    "The requested LimitExceedance record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.InitiateResponse(
                UserId.FromGuid(request.ResponseUserId),
                request.Notes,
                UtcTimestamp.From(request.RequestedAtUtc),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<LimitExceedanceActionResult>.Success(new LimitExceedanceActionResult(
                aggregate.LimitExceedanceId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute InitiateResponse for LimitExceedance {LimitExceedanceId}", request.LimitExceedanceId);

            return Result<LimitExceedanceActionResult>.Failure(new ApplicationError(
                "EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.InitiateResponseFailed",
                "Unable to InitiateResponse the LimitExceedance record at this time.",
                Guid.NewGuid()));
        }
    }
}