// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RegisterAutonomousVehicleAssetCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Interfaces;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Enums;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Aggregates.AutonomousVehicleAsset;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AutonomousVehicleSafety.Commands;

/// <summary>Handles <see cref="RegisterAutonomousVehicleAssetCommand"/>.</summary>
public sealed class RegisterAutonomousVehicleAssetCommandHandler : IRequestHandler<RegisterAutonomousVehicleAssetCommand, Result<AutonomousVehicleAssetActionResult>>
{
    private readonly IAutonomousVehicleAssetRepository _repository;
    private readonly ILogger<RegisterAutonomousVehicleAssetCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RegisterAutonomousVehicleAssetCommandHandler(IAutonomousVehicleAssetRepository repository, ILogger<RegisterAutonomousVehicleAssetCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AutonomousVehicleAssetActionResult>> Handle(RegisterAutonomousVehicleAssetCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            if (!Enum.TryParse<AutonomousVehicleType>(request.VehicleType, ignoreCase: true, out var vehicleType))
                vehicleType = AutonomousVehicleType.Amr;

            if (!Enum.TryParse<AutonomyLevel>(request.AutonomyLevel, ignoreCase: true, out var autonomyLevel))
                autonomyLevel = AutonomyLevel.SemiAutonomous;

            var aggregate = Agg.Create(
                createdBy: performedBy,
                createdAtUtc: UtcTimestamp.From(request.RequestedAtUtc),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                assetTag: request.AssetTag,
                vehicleType: vehicleType,
                autonomyLevel: autonomyLevel);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<AutonomousVehicleAssetActionResult>.Success(new AutonomousVehicleAssetActionResult(
                aggregate.AutonomousVehicleAssetId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register AutonomousVehicleAsset for Tenant {TenantId}", request.TenantId);

            return Result<AutonomousVehicleAssetActionResult>.Failure(new ApplicationError(
                "AutonomousVehicleSafety.AutonomousVehicleAsset.RegisterFailed",
                "Unable to register the AutonomousVehicleAsset record at this time.",
                Guid.NewGuid()));
        }
    }
}
