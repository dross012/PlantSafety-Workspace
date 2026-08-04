// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for DefineOperatingGeofenceCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Interfaces;
using PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Enums;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Aggregates.OperatingGeofence;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AutonomousVehicleSafety.Commands;

/// <summary>Handles <see cref="DefineOperatingGeofenceCommand"/>.</summary>
public sealed class DefineOperatingGeofenceCommandHandler : IRequestHandler<DefineOperatingGeofenceCommand, Result<OperatingGeofenceActionResult>>
{
    private readonly IOperatingGeofenceRepository _repository;
    private readonly ILogger<DefineOperatingGeofenceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DefineOperatingGeofenceCommandHandler(IOperatingGeofenceRepository repository, ILogger<DefineOperatingGeofenceCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<OperatingGeofenceActionResult>> Handle(DefineOperatingGeofenceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            if (!Enum.TryParse<GeofenceZoneType>(request.ZoneType, ignoreCase: true, out var zoneType))
                zoneType = GeofenceZoneType.OperatingZone;

            var aggregate = Agg.Create(
                createdBy: performedBy,
                createdAtUtc: UtcTimestamp.From(request.RequestedAtUtc),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                geofenceName: request.GeofenceName,
                zoneType: zoneType,
                boundaryDefinition: request.BoundaryDefinition,
                maxSpeedKph: request.MaxSpeedKph);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<OperatingGeofenceActionResult>.Success(new OperatingGeofenceActionResult(
                aggregate.OperatingGeofenceId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to define OperatingGeofence for Tenant {TenantId}", request.TenantId);

            return Result<OperatingGeofenceActionResult>.Failure(new ApplicationError(
                "AutonomousVehicleSafety.OperatingGeofence.DefineFailed",
                "Unable to define the OperatingGeofence record at this time.",
                Guid.NewGuid()));
        }
    }
}
