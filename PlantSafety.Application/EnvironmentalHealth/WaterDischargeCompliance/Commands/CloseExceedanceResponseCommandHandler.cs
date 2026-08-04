// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for CloseExceedanceResponseCommand.
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

/// <summary>Handles <see cref="CloseExceedanceResponseCommand"/>.</summary>
public sealed class CloseExceedanceResponseCommandHandler : IRequestHandler<CloseExceedanceResponseCommand, Result<LimitExceedanceActionResult>>
{
    private readonly ILimitExceedanceRepository _repository;
    private readonly ILogger<CloseExceedanceResponseCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseExceedanceResponseCommandHandler(ILimitExceedanceRepository repository, ILogger<CloseExceedanceResponseCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LimitExceedanceActionResult>> Handle(CloseExceedanceResponseCommand request, CancellationToken cancellationToken)
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

            aggregate.CloseResponse(
                UserId.FromGuid(request.ResponseUserId),
                request.ResolutionNotes,
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
            _logger.LogError(ex, "Failed to execute CloseResponse for LimitExceedance {LimitExceedanceId}", request.LimitExceedanceId);

            return Result<LimitExceedanceActionResult>.Failure(new ApplicationError(
                "EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.CloseResponseFailed",
                "Unable to CloseResponse the LimitExceedance record at this time.",
                Guid.NewGuid()));
        }
    }
}