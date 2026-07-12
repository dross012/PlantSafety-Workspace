// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: MediatR handler for SearchIncidentsQuery.
//   Full-text search across Incidents, returning a canonically paginated result set.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Extensions;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.SafetyEvents.Mappings.IncidentManagement;
using PlantSafety.Application.Features.SafetyEvents.Models.IncidentManagement;
using PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.Search;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

/// <summary>
/// Handles <see cref="SearchIncidentsQuery"/>.
/// Returns paginated <see cref="IncidentSummaryDto"/> records matching the search term.
/// </summary>
public sealed class SearchIncidentsQueryHandler
    : IQueryHandler<SearchIncidentsQuery, PaginatedResult<IncidentSummaryDto>>
{
    private readonly IIncidentReadRepository _readRepository;
    private readonly IncidentToIncidentSummaryDtoMappingProfile _mapper;
    private readonly ILogger<SearchIncidentsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SearchIncidentsQueryHandler(
        IIncidentReadRepository readRepository,
        IncidentToIncidentSummaryDtoMappingProfile mapper,
        ILogger<SearchIncidentsQueryHandler> logger)
    {
        _readRepository = readRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PaginatedResult<IncidentSummaryDto>> Handle(
        SearchIncidentsQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber.ClampPageNumber();
        var pageSize = request.PageSize.ClampPageSize();

        _logger.LogDebug(
            "Searching Incidents for Tenant {TenantId} CorrelationId {CorrelationId}",
            request.TenantId, request.CorrelationId);

        var (readModels, totalCount) = await _readRepository.SearchAsync(
            tenantId: request.TenantId,
            searchTerm: request.SearchTerm,
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
