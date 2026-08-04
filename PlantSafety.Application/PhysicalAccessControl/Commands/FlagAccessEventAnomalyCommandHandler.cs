// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for FlagAccessEventAnomalyCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Interfaces;
using PlantSafety.Domain.PhysicalAccessControl.AccessEvents.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PhysicalAccessControl.Commands;

/// <summary>Handles <see cref="FlagAccessEventAnomalyCommand"/>.</summary>
public sealed class FlagAccessEventAnomalyCommandHandler : IRequestHandler<FlagAccessEventAnomalyCommand, Result<AccessEventActionResult>>
{
    private readonly IAccessEventRepository _repository;
    private readonly ILogger<FlagAccessEventAnomalyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public FlagAccessEventAnomalyCommandHandler(IAccessEventRepository repository, ILogger<FlagAccessEventAnomalyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AccessEventActionResult>> Handle(FlagAccessEventAnomalyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = AccessEventId.FromGuid(request.AccessEventId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<AccessEventActionResult>.Failure(new ApplicationError(
                    "PhysicalAccessControl.AccessEvent.NotFound",
                    "The requested AccessEvent record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.FlagAnomaly(
                request.AnomalyType,
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<AccessEventActionResult>.Success(new AccessEventActionResult(
                aggregate.AccessEventId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute FlagAnomaly for AccessEvent {AccessEventId}", request.AccessEventId);

            return Result<AccessEventActionResult>.Failure(new ApplicationError(
                "PhysicalAccessControl.AccessEvent.FlagAnomalyFailed",
                "Unable to flag the AccessEvent record at this time.",
                Guid.NewGuid()));
        }
    }
}
