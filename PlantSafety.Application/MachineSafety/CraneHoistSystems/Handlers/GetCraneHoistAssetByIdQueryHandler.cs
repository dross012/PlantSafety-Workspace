// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetCraneHoistAssetByIdQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Models;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Repositories;

namespace PlantSafety.Application.MachineSafety.CraneHoistSystems.Handlers;

/// <summary>Handles <see cref="GetCraneHoistAssetByIdQuery"/>.</summary>
public sealed class GetCraneHoistAssetByIdQueryHandler : IRequestHandler<GetCraneHoistAssetByIdQuery, Result<CraneHoistSystemsItemDto?>>
{
    private readonly ICraneHoistSystemsRepository _repository;
    private readonly ILogger<GetCraneHoistAssetByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCraneHoistAssetByIdQueryHandler(ICraneHoistSystemsRepository repository, ILogger<GetCraneHoistAssetByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CraneHoistSystemsItemDto?>> Handle(GetCraneHoistAssetByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetCraneHoistSystemsItemByIdAsync(request.TenantId, request.CraneHoistSystemsId, cancellationToken);
            return Result<CraneHoistSystemsItemDto?>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve CraneHoistSystems asset {CraneHoistSystemsId} for Tenant {TenantId}", request.CraneHoistSystemsId, request.TenantId);

            return Result<CraneHoistSystemsItemDto?>.Failure(new ApplicationError(
                "MachineSafety.CraneHoistSystems.RetrievalFailed",
                "Unable to retrieve the CraneHoistSystems asset at this time.",
                Guid.NewGuid()));
        }
    }
}
