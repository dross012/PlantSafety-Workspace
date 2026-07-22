// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for ScheduleCorrectiveActionMachineGuardingCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.EmployeeSafety.MachineGuarding.Interfaces;
using PlantSafety.Domain.EmployeeSafety.MachineGuarding.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.EmployeeSafety.Commands;

/// <summary>Handles <see cref="ScheduleCorrectiveActionMachineGuardingCommand"/>.</summary>
public sealed class ScheduleCorrectiveActionMachineGuardingCommandHandler : IRequestHandler<ScheduleCorrectiveActionMachineGuardingCommand, Result<MachineGuardingActionResult>>
{
    private readonly IMachineGuardingRepository _repository;
    private readonly ILogger<ScheduleCorrectiveActionMachineGuardingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ScheduleCorrectiveActionMachineGuardingCommandHandler(IMachineGuardingRepository repository, ILogger<ScheduleCorrectiveActionMachineGuardingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MachineGuardingActionResult>> Handle(ScheduleCorrectiveActionMachineGuardingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = MachineGuardingId.FromGuid(request.MachineGuardingId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<MachineGuardingActionResult>.Failure(new ApplicationError(
                    "EmployeeSafety.MachineGuarding.NotFound",
                    "The requested MachineGuarding record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.ScheduleCorrectiveAction(
                UtcTimestamp.From(request.DueAtUtc),
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<MachineGuardingActionResult>.Success(new MachineGuardingActionResult(
                aggregate.MachineGuardingId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute ScheduleCorrectiveAction for MachineGuarding {MachineGuardingId}", request.MachineGuardingId);

            return Result<MachineGuardingActionResult>.Failure(new ApplicationError(
                "EmployeeSafety.MachineGuarding.ScheduleCorrectiveActionFailed",
                "Unable to ScheduleCorrectiveAction the MachineGuarding record at this time.",
                Guid.NewGuid()));
        }
    }
}