// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for BeginCorrectiveActionMachineGuardingInspectionsCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Interfaces;
using PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyProcedures.Commands;

/// <summary>Handles <see cref="BeginCorrectiveActionMachineGuardingInspectionsCommand"/>.</summary>
public sealed class BeginCorrectiveActionMachineGuardingInspectionsCommandHandler : IRequestHandler<BeginCorrectiveActionMachineGuardingInspectionsCommand, Result<MachineGuardingInspectionsActionResult>>
{
    private readonly IMachineGuardingInspectionsRepository _repository;
    private readonly ILogger<BeginCorrectiveActionMachineGuardingInspectionsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public BeginCorrectiveActionMachineGuardingInspectionsCommandHandler(IMachineGuardingInspectionsRepository repository, ILogger<BeginCorrectiveActionMachineGuardingInspectionsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MachineGuardingInspectionsActionResult>> Handle(BeginCorrectiveActionMachineGuardingInspectionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = MachineGuardingInspectionsId.FromGuid(request.MachineGuardingInspectionsId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<MachineGuardingInspectionsActionResult>.Failure(new ApplicationError(
                    "SafetyProcedures.MachineGuardingInspections.NotFound",
                    "The requested MachineGuardingInspections record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.BeginCorrectiveAction(
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<MachineGuardingInspectionsActionResult>.Success(new MachineGuardingInspectionsActionResult(
                aggregate.MachineGuardingInspectionsId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute BeginCorrectiveAction for MachineGuardingInspections {MachineGuardingInspectionsId}", request.MachineGuardingInspectionsId);

            return Result<MachineGuardingInspectionsActionResult>.Failure(new ApplicationError(
                "SafetyProcedures.MachineGuardingInspections.BeginCorrectiveActionFailed",
                "Unable to BeginCorrectiveAction the MachineGuardingInspections record at this time.",
                Guid.NewGuid()));
        }
    }
}