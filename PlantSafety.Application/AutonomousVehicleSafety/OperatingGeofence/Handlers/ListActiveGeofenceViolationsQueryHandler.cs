// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListActiveGeofenceViolationsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.AutonomousVehicleSafety.OperatingGeofence.Queries;
using PlantSafety.Application.AutonomousVehicleSafety.OperatingGeofence.Repositories;

namespace PlantSafety.Application.AutonomousVehicleSafety.OperatingGeofence.Handlers;

/// <summary>Handles <see cref="ListActiveGeofenceViolationsQuery"/>.</summary>
public sealed class ListActiveGeofenceViolationsQueryHandler : IRequestHandler<ListActiveGeofenceViolationsQuery, Result<OperatingGeofenceQueryResult>>
{
    private readonly IOperatingGeofenceRepository _repository;
    private readonly ILogger<ListActiveGeofenceViolationsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListActiveGeofenceViolationsQueryHandler(IOperatingGeofenceRepository repository, ILogger<ListActiveGeofenceViolationsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<OperatingGeofenceQueryResult>> Handle(ListActiveGeofenceViolationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetActiveGeofenceViolationsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<OperatingGeofenceQueryResult>.Success(new OperatingGeofenceQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve active geofence violations for Tenant {TenantId}", request.TenantId);

            return Result<OperatingGeofenceQueryResult>.Failure(new ApplicationError(
                "AutonomousVehicleSafety.OperatingGeofence.RetrievalFailed",
                "Unable to retrieve active geofence violations at this time.",
                Guid.NewGuid()));
        }
    }
}
