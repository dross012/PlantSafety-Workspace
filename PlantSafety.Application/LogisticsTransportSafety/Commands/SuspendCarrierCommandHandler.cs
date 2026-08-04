// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for SuspendCarrierCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.LogisticsTransportSafety.Carriers.Interfaces;
using PlantSafety.Domain.LogisticsTransportSafety.Carriers.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.LogisticsTransportSafety.Commands;

/// <summary>Handles <see cref="SuspendCarrierCommand"/>.</summary>
public sealed class SuspendCarrierCommandHandler : IRequestHandler<SuspendCarrierCommand, Result<CarrierActionResult>>
{
    private readonly ICarrierRepository _repository;
    private readonly ILogger<SuspendCarrierCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SuspendCarrierCommandHandler(ICarrierRepository repository, ILogger<SuspendCarrierCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CarrierActionResult>> Handle(SuspendCarrierCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = CarrierId.FromGuid(request.CarrierId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<CarrierActionResult>.Failure(new ApplicationError(
                    "LogisticsTransportSafety.Carriers.NotFound",
                    "The requested Carrier record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Suspend(
                request.Reason,
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<CarrierActionResult>.Success(new CarrierActionResult(
                aggregate.CarrierId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Suspend for Carrier {CarrierId}", request.CarrierId);

            return Result<CarrierActionResult>.Failure(new ApplicationError(
                "LogisticsTransportSafety.Carriers.SuspendFailed",
                "Unable to Suspend the Carrier record at this time.",
                Guid.NewGuid()));
        }
    }
}
