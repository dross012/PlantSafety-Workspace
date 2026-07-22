// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RecordTriggerEventPresenceSensingDevicesCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Interfaces;
using PlantSafety.Domain.MachineSafety.PresenceSensingDevices.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.MachineSafety.Commands;

/// <summary>Handles <see cref="RecordTriggerEventPresenceSensingDevicesCommand"/>.</summary>
public sealed class RecordTriggerEventPresenceSensingDevicesCommandHandler : IRequestHandler<RecordTriggerEventPresenceSensingDevicesCommand, Result<PresenceSensingDevicesActionResult>>
{
    private readonly IPresenceSensingDevicesRepository _repository;
    private readonly ILogger<RecordTriggerEventPresenceSensingDevicesCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordTriggerEventPresenceSensingDevicesCommandHandler(IPresenceSensingDevicesRepository repository, ILogger<RecordTriggerEventPresenceSensingDevicesCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PresenceSensingDevicesActionResult>> Handle(RecordTriggerEventPresenceSensingDevicesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PresenceSensingDevicesId.FromGuid(request.PresenceSensingDevicesId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<PresenceSensingDevicesActionResult>.Failure(new ApplicationError(
                    "MachineSafety.PresenceSensingDevices.NotFound",
                    "The requested PresenceSensingDevices record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.RecordTriggerEvent(
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<PresenceSensingDevicesActionResult>.Success(new PresenceSensingDevicesActionResult(
                aggregate.PresenceSensingDevicesId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute RecordTriggerEvent for PresenceSensingDevices {PresenceSensingDevicesId}", request.PresenceSensingDevicesId);

            return Result<PresenceSensingDevicesActionResult>.Failure(new ApplicationError(
                "MachineSafety.PresenceSensingDevices.RecordTriggerEventFailed",
                "Unable to RecordTriggerEvent the PresenceSensingDevices record at this time.",
                Guid.NewGuid()));
        }
    }
}