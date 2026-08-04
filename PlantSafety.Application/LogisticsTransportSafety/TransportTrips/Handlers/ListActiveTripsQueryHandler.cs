// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListActiveTripsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.LogisticsTransportSafety.TransportTrips.Queries;
using PlantSafety.Application.LogisticsTransportSafety.TransportTrips.Repositories;

namespace PlantSafety.Application.LogisticsTransportSafety.TransportTrips.Handlers;

/// <summary>Handles <see cref="ListActiveTripsQuery"/>.</summary>
public sealed class ListActiveTripsQueryHandler : IRequestHandler<ListActiveTripsQuery, Result<TransportTripQueryResult>>
{
    private readonly ITransportTripRepository _repository;
    private readonly ILogger<ListActiveTripsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListActiveTripsQueryHandler(ITransportTripRepository repository, ILogger<ListActiveTripsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TransportTripQueryResult>> Handle(ListActiveTripsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListActiveTripsAsync(request.TenantId, request.SiteId, cancellationToken);

            return Result<TransportTripQueryResult>.Success(new TransportTripQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list active TransportTrips for Tenant {TenantId}", request.TenantId);

            return Result<TransportTripQueryResult>.Failure(new ApplicationError(
                "LogisticsTransportSafety.TransportTrips.RetrievalFailed",
                "Unable to list active TransportTrip records at this time.",
                Guid.NewGuid()));
        }
    }
}
