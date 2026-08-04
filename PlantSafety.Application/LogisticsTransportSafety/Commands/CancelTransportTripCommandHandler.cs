// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for CancelTransportTripCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Interfaces;
using PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.LogisticsTransportSafety.Commands;

/// <summary>Handles <see cref="CancelTransportTripCommand"/>.</summary>
public sealed class CancelTransportTripCommandHandler : IRequestHandler<CancelTransportTripCommand, Result<TransportTripActionResult>>
{
    private readonly ITransportTripRepository _repository;
    private readonly ILogger<CancelTransportTripCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CancelTransportTripCommandHandler(ITransportTripRepository repository, ILogger<CancelTransportTripCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TransportTripActionResult>> Handle(CancelTransportTripCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = TransportTripId.FromGuid(request.TransportTripId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<TransportTripActionResult>.Failure(new ApplicationError(
                    "LogisticsTransportSafety.TransportTrips.NotFound",
                    "The requested TransportTrip record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Cancel(
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<TransportTripActionResult>.Success(new TransportTripActionResult(
                aggregate.TransportTripId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Cancel for TransportTrip {TransportTripId}", request.TransportTripId);

            return Result<TransportTripActionResult>.Failure(new ApplicationError(
                "LogisticsTransportSafety.TransportTrips.CancelFailed",
                "Unable to Cancel the TransportTrip record at this time.",
                Guid.NewGuid()));
        }
    }
}
