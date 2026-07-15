// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: MediatR handler for ListCrisisEventsQuery.
//   Returns a canonically paginated result set per §XVIII.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Extensions;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.SafetyEvents.Models.CrisisManagement;
using PlantSafety.Application.Features.SafetyEvents.Queries.CrisisManagement.List;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.CrisisManagement;

/// <summary>
/// Handles <see cref="ListCrisisEventsQuery"/>.
/// Returns paginated <see cref="CrisisEventDto"/> records.
/// </summary>
public sealed class ListCrisisEventsQueryHandler
    : IQueryHandler<ListCrisisEventsQuery, PaginatedResult<CrisisEventDto>>
{
    private readonly ICrisisEventRepository _repository;
    private readonly ILogger<ListCrisisEventsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListCrisisEventsQueryHandler(
        ICrisisEventRepository repository,
        ILogger<ListCrisisEventsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PaginatedResult<CrisisEventDto>> Handle(
        ListCrisisEventsQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber.ClampPageNumber();
        var pageSize = request.PageSize.ClampPageSize();
        var orderBy = request.OrderBy.SafeOrderBy("DeclaredAtUtc");
        var sortDirection = request.SortDirection.SafeSortDirection();

        var (readModels, totalCount) = await _repository.ListAsync(
            tenantId: request.TenantId,
            siteId: request.SiteId,
            status: request.Status,
            crisisCategory: request.CrisisCategory,
            orderBy: orderBy,
            sortDirection: sortDirection,
            pageNumber: pageNumber,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        var items = readModels.Select(Map).ToList();

        _logger.LogDebug(
            "Listed {Count} CrisisEvents for Tenant {TenantId} CorrelationId {CorrelationId}",
            items.Count, request.TenantId, request.CorrelationId);

        return new PaginatedResult<CrisisEventDto>(
            items: items,
            totalCount: totalCount,
            pageNumber: pageNumber,
            pageSize: pageSize);
    }

    /// <summary>Explicit, deterministic, one-way mapping from the read model to the DTO.</summary>
    private static CrisisEventDto Map(CrisisEventReadModel readModel) => new()
    {
        CrisisEventId = readModel.CrisisEventId,
        TenantId = readModel.TenantId,
        SiteId = readModel.SiteId,
        Title = readModel.Title,
        Description = readModel.Description,
        CrisisCategory = readModel.CrisisCategory,
        DeclaredAtUtc = readModel.DeclaredAtUtc,
        Status = readModel.Status,
        IncidentCommanderId = readModel.IncidentCommanderId,
        ResolvedAtUtc = readModel.ResolvedAtUtc,
        LastModifiedAtUtc = readModel.LastModifiedAtUtc,
        ETag = readModel.ETag
    };
}
