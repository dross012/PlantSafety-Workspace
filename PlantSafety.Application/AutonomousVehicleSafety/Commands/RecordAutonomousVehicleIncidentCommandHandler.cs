// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RecordAutonomousVehicleIncidentCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Interfaces;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Enums;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Aggregates.AutonomousVehicleIncident;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AutonomousVehicleSafety.Commands;

/// <summary>Handles <see cref="RecordAutonomousVehicleIncidentCommand"/>.</summary>
public sealed class RecordAutonomousVehicleIncidentCommandHandler : IRequestHandler<RecordAutonomousVehicleIncidentCommand, Result<AutonomousVehicleIncidentActionResult>>
{
    private readonly IAutonomousVehicleIncidentRepository _repository;
    private readonly ILogger<RecordAutonomousVehicleIncidentCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordAutonomousVehicleIncidentCommandHandler(IAutonomousVehicleIncidentRepository repository, ILogger<RecordAutonomousVehicleIncidentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AutonomousVehicleIncidentActionResult>> Handle(RecordAutonomousVehicleIncidentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            if (!Enum.TryParse<AutonomousVehicleIncidentType>(request.IncidentType, ignoreCase: true, out var incidentType))
                incidentType = AutonomousVehicleIncidentType.CollisionNearMiss;

            if (!Enum.TryParse<IncidentSeverityLevel>(request.SeverityLevel, ignoreCase: true, out var severityLevel))
                severityLevel = IncidentSeverityLevel.Medium;

            var aggregate = Agg.Create(
                createdBy: performedBy,
                createdAtUtc: UtcTimestamp.From(request.RequestedAtUtc),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                autonomousVehicleAssetId: request.AutonomousVehicleAssetId,
                incidentType: incidentType,
                severityLevel: severityLevel,
                descriptionSummary: request.DescriptionSummary);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<AutonomousVehicleIncidentActionResult>.Success(new AutonomousVehicleIncidentActionResult(
                aggregate.AutonomousVehicleIncidentId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record AutonomousVehicleIncident for Tenant {TenantId}", request.TenantId);

            return Result<AutonomousVehicleIncidentActionResult>.Failure(new ApplicationError(
                "AutonomousVehicleSafety.AutonomousVehicleIncident.RecordFailed",
                "Unable to record the autonomous vehicle incident at this time.",
                Guid.NewGuid()));
        }
    }
}
