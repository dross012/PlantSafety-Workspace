// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for CompleteTransportTripCommand.
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

/// <summary>Handles <see cref="CompleteTransportTripCommand"/>.</summary>
public sealed class CompleteTransportTripCommandHandler : IRequestHandler<CompleteTransportTripCommand, Result<TransportTripActionResult>>
{
    private readonly ITransportTripRepository _repository;
    private readonly ILogger<CompleteTransportTripCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CompleteTransportTripCommandHandler(ITransportTripRepository repository, ILogger<CompleteTransportTripCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TransportTripActionResult>> Handle(CompleteTransportTripCommand request, CancellationToken cancellationToken)
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

            aggregate.Complete(
                UtcTimestamp.From(request.ActualDepartureAtUtc),
                UtcTimestamp.From(request.ActualArrivalAtUtc),
                request.DistanceMiles,
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<TransportTripActionResult>.Success(new TransportTripActionResult(
                aggregate.TransportTripId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Complete for TransportTrip {TransportTripId}", request.TransportTripId);

            return Result<TransportTripActionResult>.Failure(new ApplicationError(
                "LogisticsTransportSafety.TransportTrips.CompleteFailed",
                "Unable to Complete the TransportTrip record at this time.",
                Guid.NewGuid()));
        }
    }
}
