// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: MediatR handler for ListIncidentsQuery.
//   Returns a canonically paginated result set per §XVIII.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Extensions;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.SafetyEvents.Mappings.IncidentManagement;
using PlantSafety.Application.Features.SafetyEvents.Models.IncidentManagement;
using PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.List;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

/// <summary>
/// Handles <see cref="ListIncidentsQuery"/>.
/// Returns paginated <see cref="IncidentSummaryDto"/> records.
/// </summary>
public sealed class ListIncidentsQueryHandler
    : IQueryHandler<ListIncidentsQuery, PaginatedResult<IncidentSummaryDto>>
{
    private readonly IIncidentReadRepository _readRepository;
    private readonly IncidentToIncidentSummaryDtoMappingProfile _mapper;
    private readonly ILogger<ListIncidentsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListIncidentsQueryHandler(
        IIncidentReadRepository readRepository,
        IncidentToIncidentSummaryDtoMappingProfile mapper,
        ILogger<ListIncidentsQueryHandler> logger)
    {
        _readRepository = readRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PaginatedResult<IncidentSummaryDto>> Handle(
        ListIncidentsQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber.ClampPageNumber();
        var pageSize = request.PageSize.ClampPageSize();
        var orderBy = request.OrderBy.SafeOrderBy("IncidentOccurredAtUtc");
        var sortDirection = request.SortDirection.SafeSortDirection();

        var (readModels, totalCount) = await _readRepository.ListAsync(
            tenantId: request.TenantId,
            siteId: request.SiteId,
            status: request.Status,
            severity: request.Severity,
            occurredFromUtc: request.OccurredFromUtc,
            occurredToUtc: request.OccurredToUtc,
            orderBy: orderBy,
            sortDirection: sortDirection,
            pageNumber: pageNumber,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        var items = readModels.Select(_mapper.Map).ToList();

        return new PaginatedResult<IncidentSummaryDto>(
            items: items,
            totalCount: totalCount,
            pageNumber: pageNumber,
            pageSize: pageSize);
    }
}
