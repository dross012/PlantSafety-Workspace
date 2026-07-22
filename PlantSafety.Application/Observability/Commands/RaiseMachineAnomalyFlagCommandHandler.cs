// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RaiseMachineAnomalyFlagCommand.
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

/// <summary>Handles <see cref="RaiseMachineAnomalyFlagCommand"/>.</summary>
public sealed class RaiseMachineAnomalyFlagCommandHandler : IRequestHandler<RaiseMachineAnomalyFlagCommand, Result<MachineProcessMonitoringActionResult>>
{
    private readonly IMachineProcessMonitoringRepository _repository;
    private readonly ILogger<RaiseMachineAnomalyFlagCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RaiseMachineAnomalyFlagCommandHandler(IMachineProcessMonitoringRepository repository, ILogger<RaiseMachineAnomalyFlagCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MachineProcessMonitoringActionResult>> Handle(RaiseMachineAnomalyFlagCommand request, CancellationToken cancellationToken)
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

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.RaiseAnomalyFlag(
                anomalyDescription: request.AnomalyDescription,
                flaggedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<MachineProcessMonitoringActionResult>.Success(new MachineProcessMonitoringActionResult(
                aggregate.MachineProcessMonitoringId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to raise anomaly flag for MachineProcessMonitoring {MachineProcessMonitoringId}", request.MachineProcessMonitoringId);

            return Result<MachineProcessMonitoringActionResult>.Failure(new ApplicationError(
                "Observability.MachineProcessMonitoring.RaiseAnomalyFlagFailed",
                "Unable to raise the anomaly flag at this time.",
                Guid.NewGuid()));
        }
    }
}
