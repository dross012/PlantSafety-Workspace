// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListLiftEventsByAssetQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Models;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Repositories;

namespace PlantSafety.Application.MachineSafety.CraneHoistSystems.Handlers;

/// <summary>Handles <see cref="ListLiftEventsByAssetQuery"/>.</summary>
public sealed class ListLiftEventsByAssetQueryHandler : IRequestHandler<ListLiftEventsByAssetQuery, Result<IReadOnlyList<CraneHoistSystemsItemDto>>>
{
    private readonly ICraneHoistSystemsRepository _repository;
    private readonly ILogger<ListLiftEventsByAssetQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListLiftEventsByAssetQueryHandler(ICraneHoistSystemsRepository repository, ILogger<ListLiftEventsByAssetQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<CraneHoistSystemsItemDto>>> Handle(ListLiftEventsByAssetQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetCraneHoistSystemsItemByIdAsync(request.TenantId, request.CraneHoistSystemsId, cancellationToken);
            IReadOnlyList<CraneHoistSystemsItemDto> items = item is null ? [] : [item];
            return Result<IReadOnlyList<CraneHoistSystemsItemDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list lift events for CraneHoistSystems asset {CraneHoistSystemsId}", request.CraneHoistSystemsId);

            return Result<IReadOnlyList<CraneHoistSystemsItemDto>>.Failure(new ApplicationError(
                "MachineSafety.CraneHoistSystems.ListLiftEventsFailed",
                "Unable to list lift events for the CraneHoistSystems asset at this time.",
                Guid.NewGuid()));
        }
    }
}
