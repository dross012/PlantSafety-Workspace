// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetIncidentHistoryByAssetQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleIncident.Queries;
using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleIncident.Repositories;

namespace PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleIncident.Handlers;

/// <summary>Handles <see cref="GetIncidentHistoryByAssetQuery"/>.</summary>
public sealed class GetIncidentHistoryByAssetQueryHandler : IRequestHandler<GetIncidentHistoryByAssetQuery, Result<AutonomousVehicleIncidentQueryResult>>
{
    private readonly IAutonomousVehicleIncidentRepository _repository;
    private readonly ILogger<GetIncidentHistoryByAssetQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetIncidentHistoryByAssetQueryHandler(IAutonomousVehicleIncidentRepository repository, ILogger<GetIncidentHistoryByAssetQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AutonomousVehicleIncidentQueryResult>> Handle(GetIncidentHistoryByAssetQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetIncidentHistoryByAssetIdAsync(request.TenantId, request.AutonomousVehicleAssetId, cancellationToken);

            return Result<AutonomousVehicleIncidentQueryResult>.Success(new AutonomousVehicleIncidentQueryResult(
                request.TenantId,
                request.AutonomousVehicleAssetId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve incident history for Asset {AutonomousVehicleAssetId}, Tenant {TenantId}", request.AutonomousVehicleAssetId, request.TenantId);

            return Result<AutonomousVehicleIncidentQueryResult>.Failure(new ApplicationError(
                "AutonomousVehicleSafety.AutonomousVehicleIncident.RetrievalFailed",
                "Unable to retrieve the incident history at this time.",
                Guid.NewGuid()));
        }
    }
}
