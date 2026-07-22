// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RecordInspectionResultMachineGuardsBarriersCommand.
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

/// <summary>Handles <see cref="RecordInspectionResultMachineGuardsBarriersCommand"/>.</summary>
public sealed class RecordInspectionResultMachineGuardsBarriersCommandHandler : IRequestHandler<RecordInspectionResultMachineGuardsBarriersCommand, Result<MachineGuardsBarriersActionResult>>
{
    private readonly IMachineGuardsBarriersRepository _repository;
    private readonly ILogger<RecordInspectionResultMachineGuardsBarriersCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordInspectionResultMachineGuardsBarriersCommandHandler(IMachineGuardsBarriersRepository repository, ILogger<RecordInspectionResultMachineGuardsBarriersCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MachineGuardsBarriersActionResult>> Handle(RecordInspectionResultMachineGuardsBarriersCommand request, CancellationToken cancellationToken)
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

            aggregate.RecordInspectionResult(
                request.DeficiencyCount,
                request.GuardIntegrityScore,
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
            _logger.LogError(ex, "Failed to execute RecordInspectionResult for MachineGuardsBarriers {MachineGuardsBarriersId}", request.MachineGuardsBarriersId);

            return Result<MachineGuardsBarriersActionResult>.Failure(new ApplicationError(
                "MachineSafety.MachineGuardsBarriers.RecordInspectionResultFailed",
                "Unable to RecordInspectionResult the MachineGuardsBarriers record at this time.",
                Guid.NewGuid()));
        }
    }
}