// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: MediatR handler for ListDrillsQuery.
//   Returns a canonically paginated result set per §XVIII.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Extensions;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.SafetyEvents.Models.DrillsAndExercises;
using PlantSafety.Application.Features.SafetyEvents.Queries.DrillsAndExercises.List;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.DrillsAndExercises;

/// <summary>
/// Handles <see cref="ListDrillsQuery"/>.
/// Returns paginated <see cref="DrillDto"/> records.
/// </summary>
public sealed class ListDrillsQueryHandler : IQueryHandler<ListDrillsQuery, PaginatedResult<DrillDto>>
{
    private readonly IDrillRepository _repository;
    private readonly ILogger<ListDrillsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListDrillsQueryHandler(IDrillRepository repository, ILogger<ListDrillsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PaginatedResult<DrillDto>> Handle(ListDrillsQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber.ClampPageNumber();
        var pageSize = request.PageSize.ClampPageSize();
        var orderBy = request.OrderBy.SafeOrderBy("ScheduledAtUtc");
        var sortDirection = request.SortDirection.SafeSortDirection();

        var (readModels, totalCount) = await _repository.ListAsync(
            tenantId: request.TenantId,
            siteId: request.SiteId,
            status: request.Status,
            drillType: request.DrillType,
            orderBy: orderBy,
            sortDirection: sortDirection,
            pageNumber: pageNumber,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        var items = readModels.Select(Map).ToList();

        _logger.LogDebug(
            "Listed {Count} Drills for Tenant {TenantId} CorrelationId {CorrelationId}",
            items.Count, request.TenantId, request.CorrelationId);

        return new PaginatedResult<DrillDto>(
            items: items,
            totalCount: totalCount,
            pageNumber: pageNumber,
            pageSize: pageSize);
    }

    /// <summary>Explicit, deterministic, one-way mapping from the read model to the DTO.</summary>
    private static DrillDto Map(DrillReadModel readModel) => new()
    {
        DrillId = readModel.DrillId,
        TenantId = readModel.TenantId,
        SiteId = readModel.SiteId,
        Title = readModel.Title,
        Description = readModel.Description,
        DrillType = readModel.DrillType,
        ScheduledAtUtc = readModel.ScheduledAtUtc,
        ExecutedAtUtc = readModel.ExecutedAtUtc,
        Status = readModel.Status,
        CoordinatorId = readModel.CoordinatorId,
        OutcomeScore = readModel.OutcomeScore,
        LastModifiedAtUtc = readModel.LastModifiedAtUtc,
        ETag = readModel.ETag
    };
}
