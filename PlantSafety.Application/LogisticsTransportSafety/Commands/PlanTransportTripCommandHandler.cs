// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for PlanTransportTripCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Enums;
using PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using Agg = PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Aggregates.TransportTrip;

namespace PlantSafety.Application.LogisticsTransportSafety.Commands;

/// <summary>Handles <see cref="PlanTransportTripCommand"/>.</summary>
public sealed class PlanTransportTripCommandHandler : IRequestHandler<PlanTransportTripCommand, Result<TransportTripActionResult>>
{
    private readonly ITransportTripRepository _repository;
    private readonly ILogger<PlanTransportTripCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public PlanTransportTripCommandHandler(ITransportTripRepository repository, ILogger<PlanTransportTripCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TransportTripActionResult>> Handle(PlanTransportTripCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            if (!Enum.TryParse<TransportTripType>(request.TripType, ignoreCase: true, out var tripType))
            {
                return Result<TransportTripActionResult>.Failure(new ApplicationError(
                    "LogisticsTransportSafety.TransportTrips.InvalidTripType",
                    $"'{request.TripType}' is not a recognised transport trip type.",
                    Guid.NewGuid()));
            }

            var aggregate = Agg.Create(
                createdBy: performedBy,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                vehicleId: request.VehicleId,
                driverUserId: request.DriverUserId,
                tripType: tripType,
                routeDescription: request.RouteDescription,
                plannedDepartureAtUtc: UtcTimestamp.From(request.PlannedDepartureAtUtc),
                plannedArrivalAtUtc: UtcTimestamp.From(request.PlannedArrivalAtUtc));

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<TransportTripActionResult>.Success(new TransportTripActionResult(
                aggregate.TransportTripId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to plan TransportTrip for Tenant {TenantId}", request.TenantId);

            return Result<TransportTripActionResult>.Failure(new ApplicationError(
                "LogisticsTransportSafety.TransportTrips.PlanFailed",
                "Unable to plan the TransportTrip at this time.",
                Guid.NewGuid()));
        }
    }
}
