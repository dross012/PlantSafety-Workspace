// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for ScheduleTestEmergencyStopDevicesCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Interfaces;
using PlantSafety.Domain.MachineSafety.EmergencyStopDevices.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.MachineSafety.Commands;

/// <summary>Handles <see cref="ScheduleTestEmergencyStopDevicesCommand"/>.</summary>
public sealed class ScheduleTestEmergencyStopDevicesCommandHandler : IRequestHandler<ScheduleTestEmergencyStopDevicesCommand, Result<EmergencyStopDevicesActionResult>>
{
    private readonly IEmergencyStopDevicesRepository _repository;
    private readonly ILogger<ScheduleTestEmergencyStopDevicesCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ScheduleTestEmergencyStopDevicesCommandHandler(IEmergencyStopDevicesRepository repository, ILogger<ScheduleTestEmergencyStopDevicesCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencyStopDevicesActionResult>> Handle(ScheduleTestEmergencyStopDevicesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = EmergencyStopDevicesId.FromGuid(request.EmergencyStopDevicesId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<EmergencyStopDevicesActionResult>.Failure(new ApplicationError(
                    "MachineSafety.EmergencyStopDevices.NotFound",
                    "The requested EmergencyStopDevices record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.ScheduleTest(
                UtcTimestamp.From(request.DueAtUtc),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<EmergencyStopDevicesActionResult>.Success(new EmergencyStopDevicesActionResult(
                aggregate.EmergencyStopDevicesId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute ScheduleTest for EmergencyStopDevices {EmergencyStopDevicesId}", request.EmergencyStopDevicesId);

            return Result<EmergencyStopDevicesActionResult>.Failure(new ApplicationError(
                "MachineSafety.EmergencyStopDevices.ScheduleTestFailed",
                "Unable to ScheduleTest the EmergencyStopDevices record at this time.",
                Guid.NewGuid()));
        }
    }
}