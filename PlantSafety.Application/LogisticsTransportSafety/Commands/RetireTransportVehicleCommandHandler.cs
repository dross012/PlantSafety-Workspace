// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RetireTransportVehicleCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Interfaces;
using PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.LogisticsTransportSafety.Commands;

/// <summary>Handles <see cref="RetireTransportVehicleCommand"/>.</summary>
public sealed class RetireTransportVehicleCommandHandler : IRequestHandler<RetireTransportVehicleCommand, Result<TransportVehicleActionResult>>
{
    private readonly ITransportVehicleRepository _repository;
    private readonly ILogger<RetireTransportVehicleCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RetireTransportVehicleCommandHandler(ITransportVehicleRepository repository, ILogger<RetireTransportVehicleCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TransportVehicleActionResult>> Handle(RetireTransportVehicleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = TransportVehicleId.FromGuid(request.TransportVehicleId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<TransportVehicleActionResult>.Failure(new ApplicationError(
                    "LogisticsTransportSafety.TransportVehicles.NotFound",
                    "The requested TransportVehicle record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Retire(
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<TransportVehicleActionResult>.Success(new TransportVehicleActionResult(
                aggregate.TransportVehicleId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Retire for TransportVehicle {TransportVehicleId}", request.TransportVehicleId);

            return Result<TransportVehicleActionResult>.Failure(new ApplicationError(
                "LogisticsTransportSafety.TransportVehicles.RetireFailed",
                "Unable to Retire the TransportVehicle record at this time.",
                Guid.NewGuid()));
        }
    }
}
