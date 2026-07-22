// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for CloseMachineGuardingInspectionsCommand.
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

/// <summary>Handles <see cref="CloseMachineGuardingInspectionsCommand"/>.</summary>
public sealed class CloseMachineGuardingInspectionsCommandHandler : IRequestHandler<CloseMachineGuardingInspectionsCommand, Result<MachineGuardingInspectionsActionResult>>
{
    private readonly IMachineGuardingInspectionsRepository _repository;
    private readonly ILogger<CloseMachineGuardingInspectionsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseMachineGuardingInspectionsCommandHandler(IMachineGuardingInspectionsRepository repository, ILogger<CloseMachineGuardingInspectionsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MachineGuardingInspectionsActionResult>> Handle(CloseMachineGuardingInspectionsCommand request, CancellationToken cancellationToken)
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

            aggregate.Close(
                UtcTimestamp.From(request.NextDueAtUtc),
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
            _logger.LogError(ex, "Failed to execute Close for MachineGuardingInspections {MachineGuardingInspectionsId}", request.MachineGuardingInspectionsId);

            return Result<MachineGuardingInspectionsActionResult>.Failure(new ApplicationError(
                "SafetyProcedures.MachineGuardingInspections.CloseFailed",
                "Unable to Close the MachineGuardingInspections record at this time.",
                Guid.NewGuid()));
        }
    }
}