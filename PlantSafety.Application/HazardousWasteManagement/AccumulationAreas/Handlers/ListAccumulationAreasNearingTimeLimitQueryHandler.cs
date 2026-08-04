// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListAccumulationAreasNearingTimeLimitQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.HazardousWasteManagement.AccumulationAreas.Queries;
using PlantSafety.Application.HazardousWasteManagement.AccumulationAreas.Repositories;

namespace PlantSafety.Application.HazardousWasteManagement.AccumulationAreas.Handlers;

/// <summary>Handles <see cref="ListAccumulationAreasNearingTimeLimitQuery"/>.</summary>
public sealed class ListAccumulationAreasNearingTimeLimitQueryHandler : IRequestHandler<ListAccumulationAreasNearingTimeLimitQuery, Result<ListAccumulationAreasNearingTimeLimitResult>>
{
    private readonly IAccumulationAreaRepository _repository;
    private readonly ILogger<ListAccumulationAreasNearingTimeLimitQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListAccumulationAreasNearingTimeLimitQueryHandler(IAccumulationAreaRepository repository, ILogger<ListAccumulationAreasNearingTimeLimitQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ListAccumulationAreasNearingTimeLimitResult>> Handle(ListAccumulationAreasNearingTimeLimitQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListAccumulationAreasNearingTimeLimitAsync(request.TenantId, request.SiteId, cancellationToken);
            return Result<ListAccumulationAreasNearingTimeLimitResult>.Success(new ListAccumulationAreasNearingTimeLimitResult(items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list accumulation areas nearing time limit for Tenant {TenantId}", request.TenantId);

            return Result<ListAccumulationAreasNearingTimeLimitResult>.Failure(new ApplicationError(
                "HazardousWasteManagement.AccumulationArea.ListNearingTimeLimitFailed",
                "Unable to list accumulation areas nearing their time limit at this time.",
                Guid.NewGuid()));
        }
    }
}
