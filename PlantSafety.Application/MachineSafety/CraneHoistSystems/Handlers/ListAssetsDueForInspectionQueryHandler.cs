// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListAssetsDueForInspectionQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Models;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Repositories;

namespace PlantSafety.Application.MachineSafety.CraneHoistSystems.Handlers;

/// <summary>Handles <see cref="ListAssetsDueForInspectionQuery"/>.</summary>
public sealed class ListAssetsDueForInspectionQueryHandler : IRequestHandler<ListAssetsDueForInspectionQuery, Result<IReadOnlyList<CraneHoistSystemsItemDto>>>
{
    private readonly ICraneHoistSystemsRepository _repository;
    private readonly ILogger<ListAssetsDueForInspectionQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListAssetsDueForInspectionQueryHandler(ICraneHoistSystemsRepository repository, ILogger<ListAssetsDueForInspectionQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<CraneHoistSystemsItemDto>>> Handle(ListAssetsDueForInspectionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListAssetsDueForInspectionAsync(request.TenantId, request.SiteId, request.AsOfUtc, cancellationToken);
            return Result<IReadOnlyList<CraneHoistSystemsItemDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list CraneHoistSystems assets due for inspection for Tenant {TenantId}", request.TenantId);

            return Result<IReadOnlyList<CraneHoistSystemsItemDto>>.Failure(new ApplicationError(
                "MachineSafety.CraneHoistSystems.ListDueForInspectionFailed",
                "Unable to list CraneHoistSystems assets due for inspection at this time.",
                Guid.NewGuid()));
        }
    }
}
