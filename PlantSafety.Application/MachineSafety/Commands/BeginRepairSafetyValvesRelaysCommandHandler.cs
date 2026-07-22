// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for BeginRepairSafetyValvesRelaysCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Interfaces;
using PlantSafety.Domain.MachineSafety.SafetyValvesRelays.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.MachineSafety.Commands;

/// <summary>Handles <see cref="BeginRepairSafetyValvesRelaysCommand"/>.</summary>
public sealed class BeginRepairSafetyValvesRelaysCommandHandler : IRequestHandler<BeginRepairSafetyValvesRelaysCommand, Result<SafetyValvesRelaysActionResult>>
{
    private readonly ISafetyValvesRelaysRepository _repository;
    private readonly ILogger<BeginRepairSafetyValvesRelaysCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public BeginRepairSafetyValvesRelaysCommandHandler(ISafetyValvesRelaysRepository repository, ILogger<BeginRepairSafetyValvesRelaysCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SafetyValvesRelaysActionResult>> Handle(BeginRepairSafetyValvesRelaysCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = SafetyValvesRelaysId.FromGuid(request.SafetyValvesRelaysId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<SafetyValvesRelaysActionResult>.Failure(new ApplicationError(
                    "MachineSafety.SafetyValvesRelays.NotFound",
                    "The requested SafetyValvesRelays record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.BeginRepair(
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<SafetyValvesRelaysActionResult>.Success(new SafetyValvesRelaysActionResult(
                aggregate.SafetyValvesRelaysId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute BeginRepair for SafetyValvesRelays {SafetyValvesRelaysId}", request.SafetyValvesRelaysId);

            return Result<SafetyValvesRelaysActionResult>.Failure(new ApplicationError(
                "MachineSafety.SafetyValvesRelays.BeginRepairFailed",
                "Unable to BeginRepair the SafetyValvesRelays record at this time.",
                Guid.NewGuid()));
        }
    }
}