// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for VerifyCorrectiveActionMachineGuardsBarriersCommand.
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

/// <summary>Handles <see cref="VerifyCorrectiveActionMachineGuardsBarriersCommand"/>.</summary>
public sealed class VerifyCorrectiveActionMachineGuardsBarriersCommandHandler : IRequestHandler<VerifyCorrectiveActionMachineGuardsBarriersCommand, Result<MachineGuardsBarriersActionResult>>
{
    private readonly IMachineGuardsBarriersRepository _repository;
    private readonly ILogger<VerifyCorrectiveActionMachineGuardsBarriersCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public VerifyCorrectiveActionMachineGuardsBarriersCommandHandler(IMachineGuardsBarriersRepository repository, ILogger<VerifyCorrectiveActionMachineGuardsBarriersCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MachineGuardsBarriersActionResult>> Handle(VerifyCorrectiveActionMachineGuardsBarriersCommand request, CancellationToken cancellationToken)
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

            aggregate.VerifyCorrectiveAction(
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
            _logger.LogError(ex, "Failed to execute VerifyCorrectiveAction for MachineGuardsBarriers {MachineGuardsBarriersId}", request.MachineGuardsBarriersId);

            return Result<MachineGuardsBarriersActionResult>.Failure(new ApplicationError(
                "MachineSafety.MachineGuardsBarriers.VerifyCorrectiveActionFailed",
                "Unable to VerifyCorrectiveAction the MachineGuardsBarriers record at this time.",
                Guid.NewGuid()));
        }
    }
}