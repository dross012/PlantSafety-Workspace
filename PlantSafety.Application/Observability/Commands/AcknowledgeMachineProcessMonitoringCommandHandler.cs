// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for AcknowledgeMachineProcessMonitoringCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Observability.MachineProcessMonitoring.Interfaces;
using PlantSafety.Domain.Observability.MachineProcessMonitoring.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Observability.Commands;

/// <summary>Handles <see cref="AcknowledgeMachineProcessMonitoringCommand"/>.</summary>
public sealed class AcknowledgeMachineProcessMonitoringCommandHandler : IRequestHandler<AcknowledgeMachineProcessMonitoringCommand, Result<MachineProcessMonitoringActionResult>>
{
    private readonly IMachineProcessMonitoringRepository _repository;
    private readonly ILogger<AcknowledgeMachineProcessMonitoringCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AcknowledgeMachineProcessMonitoringCommandHandler(IMachineProcessMonitoringRepository repository, ILogger<AcknowledgeMachineProcessMonitoringCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MachineProcessMonitoringActionResult>> Handle(AcknowledgeMachineProcessMonitoringCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = MachineProcessMonitoringId.FromGuid(request.MachineProcessMonitoringId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<MachineProcessMonitoringActionResult>.Failure(new ApplicationError(
                    "Observability.MachineProcessMonitoring.NotFound",
                    "The requested machine process monitoring channel could not be found.",
                    Guid.NewGuid()));
            }

            var acknowledgedBy = UserId.FromGuid(request.AcknowledgedByUserId);

            aggregate.Acknowledge(
                acknowledgedBy: acknowledgedBy,
                acknowledgedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(acknowledgedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<MachineProcessMonitoringActionResult>.Success(new MachineProcessMonitoringActionResult(
                aggregate.MachineProcessMonitoringId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acknowledge MachineProcessMonitoring {MachineProcessMonitoringId}", request.MachineProcessMonitoringId);

            return Result<MachineProcessMonitoringActionResult>.Failure(new ApplicationError(
                "Observability.MachineProcessMonitoring.AcknowledgeFailed",
                "Unable to acknowledge the machine process monitoring channel at this time.",
                Guid.NewGuid()));
        }
    }
}
