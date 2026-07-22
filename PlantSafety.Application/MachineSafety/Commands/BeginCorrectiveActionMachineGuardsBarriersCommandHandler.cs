// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for BeginCorrectiveActionMachineGuardsBarriersCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Interfaces;
using PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.MachineSafety.Commands;

/// <summary>Handles <see cref="BeginCorrectiveActionMachineGuardsBarriersCommand"/>.</summary>
public sealed class BeginCorrectiveActionMachineGuardsBarriersCommandHandler : IRequestHandler<BeginCorrectiveActionMachineGuardsBarriersCommand, Result<MachineGuardsBarriersActionResult>>
{
    private readonly IMachineGuardsBarriersRepository _repository;
    private readonly ILogger<BeginCorrectiveActionMachineGuardsBarriersCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public BeginCorrectiveActionMachineGuardsBarriersCommandHandler(IMachineGuardsBarriersRepository repository, ILogger<BeginCorrectiveActionMachineGuardsBarriersCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MachineGuardsBarriersActionResult>> Handle(BeginCorrectiveActionMachineGuardsBarriersCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = MachineGuardsBarriersId.FromGuid(request.MachineGuardsBarriersId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<MachineGuardsBarriersActionResult>.Failure(new ApplicationError(
                    "MachineSafety.MachineGuardsBarriers.NotFound",
                    "The requested MachineGuardsBarriers record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.BeginCorrectiveAction(
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<MachineGuardsBarriersActionResult>.Success(new MachineGuardsBarriersActionResult(
                aggregate.MachineGuardsBarriersId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute BeginCorrectiveAction for MachineGuardsBarriers {MachineGuardsBarriersId}", request.MachineGuardsBarriersId);

            return Result<MachineGuardsBarriersActionResult>.Failure(new ApplicationError(
                "MachineSafety.MachineGuardsBarriers.BeginCorrectiveActionFailed",
                "Unable to BeginCorrectiveAction the MachineGuardsBarriers record at this time.",
                Guid.NewGuid()));
        }
    }
}