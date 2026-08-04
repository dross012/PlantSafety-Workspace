// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ScheduleInspectionTransportVehicleCommand.
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

/// <summary>Handles <see cref="ScheduleInspectionTransportVehicleCommand"/>.</summary>
public sealed class ScheduleInspectionTransportVehicleCommandHandler : IRequestHandler<ScheduleInspectionTransportVehicleCommand, Result<TransportVehicleActionResult>>
{
    private readonly ITransportVehicleRepository _repository;
    private readonly ILogger<ScheduleInspectionTransportVehicleCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ScheduleInspectionTransportVehicleCommandHandler(ITransportVehicleRepository repository, ILogger<ScheduleInspectionTransportVehicleCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TransportVehicleActionResult>> Handle(ScheduleInspectionTransportVehicleCommand request, CancellationToken cancellationToken)
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

            aggregate.ScheduleInspection(
                UtcTimestamp.From(request.DueAtUtc),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<TransportVehicleActionResult>.Success(new TransportVehicleActionResult(
                aggregate.TransportVehicleId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute ScheduleInspection for TransportVehicle {TransportVehicleId}", request.TransportVehicleId);

            return Result<TransportVehicleActionResult>.Failure(new ApplicationError(
                "LogisticsTransportSafety.TransportVehicles.ScheduleInspectionFailed",
                "Unable to ScheduleInspection the TransportVehicle record at this time.",
                Guid.NewGuid()));
        }
    }
}
