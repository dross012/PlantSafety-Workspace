// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RetirePresenceSensingDevicesCommand.
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

/// <summary>Handles <see cref="RetirePresenceSensingDevicesCommand"/>.</summary>
public sealed class RetirePresenceSensingDevicesCommandHandler : IRequestHandler<RetirePresenceSensingDevicesCommand, Result<PresenceSensingDevicesActionResult>>
{
    private readonly IPresenceSensingDevicesRepository _repository;
    private readonly ILogger<RetirePresenceSensingDevicesCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RetirePresenceSensingDevicesCommandHandler(IPresenceSensingDevicesRepository repository, ILogger<RetirePresenceSensingDevicesCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PresenceSensingDevicesActionResult>> Handle(RetirePresenceSensingDevicesCommand request, CancellationToken cancellationToken)
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

            aggregate.Retire(
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
            _logger.LogError(ex, "Failed to execute Retire for PresenceSensingDevices {PresenceSensingDevicesId}", request.PresenceSensingDevicesId);

            return Result<PresenceSensingDevicesActionResult>.Failure(new ApplicationError(
                "MachineSafety.PresenceSensingDevices.RetireFailed",
                "Unable to Retire the PresenceSensingDevices record at this time.",
                Guid.NewGuid()));
        }
    }
}