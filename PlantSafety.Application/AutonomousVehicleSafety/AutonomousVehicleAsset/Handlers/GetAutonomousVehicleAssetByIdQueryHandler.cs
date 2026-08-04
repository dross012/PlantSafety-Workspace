// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetAutonomousVehicleAssetByIdQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleAsset.Models;
using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleAsset.Queries;
using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleAsset.Repositories;

namespace PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleAsset.Handlers;

/// <summary>Handles <see cref="GetAutonomousVehicleAssetByIdQuery"/>.</summary>
public sealed class GetAutonomousVehicleAssetByIdQueryHandler : IRequestHandler<GetAutonomousVehicleAssetByIdQuery, Result<AutonomousVehicleAssetItemDto?>>
{
    private readonly IAutonomousVehicleAssetRepository _repository;
    private readonly ILogger<GetAutonomousVehicleAssetByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAutonomousVehicleAssetByIdQueryHandler(IAutonomousVehicleAssetRepository repository, ILogger<GetAutonomousVehicleAssetByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AutonomousVehicleAssetItemDto?>> Handle(GetAutonomousVehicleAssetByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetAutonomousVehicleAssetByIdAsync(request.TenantId, request.AutonomousVehicleAssetId, cancellationToken);
            return Result<AutonomousVehicleAssetItemDto?>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve AutonomousVehicleAsset {AutonomousVehicleAssetId} for Tenant {TenantId}", request.AutonomousVehicleAssetId, request.TenantId);

            return Result<AutonomousVehicleAssetItemDto?>.Failure(new ApplicationError(
                "AutonomousVehicleSafety.AutonomousVehicleAsset.RetrievalFailed",
                "Unable to retrieve the AutonomousVehicleAsset record at this time.",
                Guid.NewGuid()));
        }
    }
}
