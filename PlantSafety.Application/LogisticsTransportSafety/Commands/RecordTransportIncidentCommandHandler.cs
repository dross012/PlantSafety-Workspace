// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RecordTransportIncidentCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Enums;
using PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using Agg = PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Aggregates.TransportIncident;

namespace PlantSafety.Application.LogisticsTransportSafety.Commands;

/// <summary>Handles <see cref="RecordTransportIncidentCommand"/>.</summary>
public sealed class RecordTransportIncidentCommandHandler : IRequestHandler<RecordTransportIncidentCommand, Result<TransportIncidentActionResult>>
{
    private readonly ITransportIncidentRepository _repository;
    private readonly ILogger<RecordTransportIncidentCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordTransportIncidentCommandHandler(ITransportIncidentRepository repository, ILogger<RecordTransportIncidentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TransportIncidentActionResult>> Handle(RecordTransportIncidentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            if (!Enum.TryParse<TransportIncidentType>(request.IncidentType, ignoreCase: true, out var incidentType))
            {
                return Result<TransportIncidentActionResult>.Failure(new ApplicationError(
                    "LogisticsTransportSafety.TransportIncidents.InvalidIncidentType",
                    $"'{request.IncidentType}' is not a recognised transport incident type.",
                    Guid.NewGuid()));
            }

            if (!Enum.TryParse<TransportIncidentSeverity>(request.Severity, ignoreCase: true, out var severity))
            {
                return Result<TransportIncidentActionResult>.Failure(new ApplicationError(
                    "LogisticsTransportSafety.TransportIncidents.InvalidSeverity",
                    $"'{request.Severity}' is not a recognised transport incident severity.",
                    Guid.NewGuid()));
            }

            var aggregate = Agg.Create(
                createdBy: performedBy,
                createdAtUtc: UtcTimestamp.From(request.RequestedAtUtc),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                tripId: request.TripId,
                vehicleId: request.VehicleId,
                driverUserId: request.DriverUserId,
                incidentType: incidentType,
                severity: severity,
                location: request.Location);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<TransportIncidentActionResult>.Success(new TransportIncidentActionResult(
                aggregate.TransportIncidentId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record TransportIncident for Tenant {TenantId}", request.TenantId);

            return Result<TransportIncidentActionResult>.Failure(new ApplicationError(
                "LogisticsTransportSafety.TransportIncidents.RecordFailed",
                "Unable to record the TransportIncident at this time.",
                Guid.NewGuid()));
        }
    }
}
