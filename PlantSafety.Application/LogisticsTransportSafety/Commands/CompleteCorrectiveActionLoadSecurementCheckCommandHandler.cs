// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for CompleteCorrectiveActionLoadSecurementCheckCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Interfaces;
using PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.LogisticsTransportSafety.Commands;

/// <summary>Handles <see cref="CompleteCorrectiveActionLoadSecurementCheckCommand"/>.</summary>
public sealed class CompleteCorrectiveActionLoadSecurementCheckCommandHandler : IRequestHandler<CompleteCorrectiveActionLoadSecurementCheckCommand, Result<LoadSecurementCheckActionResult>>
{
    private readonly ILoadSecurementCheckRepository _repository;
    private readonly ILogger<CompleteCorrectiveActionLoadSecurementCheckCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CompleteCorrectiveActionLoadSecurementCheckCommandHandler(ILoadSecurementCheckRepository repository, ILogger<CompleteCorrectiveActionLoadSecurementCheckCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LoadSecurementCheckActionResult>> Handle(CompleteCorrectiveActionLoadSecurementCheckCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = LoadSecurementCheckId.FromGuid(request.LoadSecurementCheckId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<LoadSecurementCheckActionResult>.Failure(new ApplicationError(
                    "LogisticsTransportSafety.LoadSecurementChecks.NotFound",
                    "The requested LoadSecurementCheck record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.CompleteCorrectiveAction(
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<LoadSecurementCheckActionResult>.Success(new LoadSecurementCheckActionResult(
                aggregate.LoadSecurementCheckId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute CompleteCorrectiveAction for LoadSecurementCheck {LoadSecurementCheckId}", request.LoadSecurementCheckId);

            return Result<LoadSecurementCheckActionResult>.Failure(new ApplicationError(
                "LogisticsTransportSafety.LoadSecurementChecks.CompleteCorrectiveActionFailed",
                "Unable to CompleteCorrectiveAction for the LoadSecurementCheck record at this time.",
                Guid.NewGuid()));
        }
    }
}
