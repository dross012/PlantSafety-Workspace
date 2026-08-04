// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RegisterTransportVehicleCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Enums;
using PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using Agg = PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Aggregates.TransportVehicle;

namespace PlantSafety.Application.LogisticsTransportSafety.Commands;

/// <summary>Handles <see cref="RegisterTransportVehicleCommand"/>.</summary>
public sealed class RegisterTransportVehicleCommandHandler : IRequestHandler<RegisterTransportVehicleCommand, Result<TransportVehicleActionResult>>
{
    private readonly ITransportVehicleRepository _repository;
    private readonly ILogger<RegisterTransportVehicleCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RegisterTransportVehicleCommandHandler(ITransportVehicleRepository repository, ILogger<RegisterTransportVehicleCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TransportVehicleActionResult>> Handle(RegisterTransportVehicleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            if (!Enum.TryParse<TransportVehicleType>(request.VehicleType, ignoreCase: true, out var vehicleType))
            {
                return Result<TransportVehicleActionResult>.Failure(new ApplicationError(
                    "LogisticsTransportSafety.TransportVehicles.InvalidVehicleType",
                    $"'{request.VehicleType}' is not a recognised transport vehicle type.",
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
                vehicleTitle: request.VehicleTitle,
                vehicleType: vehicleType,
                vehicleIdentifier: request.VehicleIdentifier);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<TransportVehicleActionResult>.Success(new TransportVehicleActionResult(
                aggregate.TransportVehicleId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register TransportVehicle for Tenant {TenantId}", request.TenantId);

            return Result<TransportVehicleActionResult>.Failure(new ApplicationError(
                "LogisticsTransportSafety.TransportVehicles.RegisterFailed",
                "Unable to register the TransportVehicle record at this time.",
                Guid.NewGuid()));
        }
    }
}
