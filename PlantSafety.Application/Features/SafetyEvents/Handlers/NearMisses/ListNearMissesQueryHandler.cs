// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for ListNearMissesQuery.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Extensions;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.SafetyEvents.Models.NearMisses;
using PlantSafety.Application.Features.SafetyEvents.Queries.NearMisses.List;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.NearMisses;

/// <summary>Handles <see cref="ListNearMissesQuery"/>. Returns paginated near miss records.</summary>
public sealed class ListNearMissesQueryHandler : IQueryHandler<ListNearMissesQuery, PaginatedResult<NearMissDto>>
{
    private readonly INearMissRepository _repository;
    private readonly ILogger<ListNearMissesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListNearMissesQueryHandler(INearMissRepository repository, ILogger<ListNearMissesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PaginatedResult<NearMissDto>> Handle(ListNearMissesQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber.ClampPageNumber();
        var pageSize = request.PageSize.ClampPageSize();

        var (models, total) = await _repository.ListAsync(
            tenantId: request.TenantId,
            siteId: request.SiteId,
            potentialSeverity: request.PotentialSeverity,
            hazardCategory: request.HazardCategory,
            occurredFromUtc: request.OccurredFromUtc,
            occurredToUtc: request.OccurredToUtc,
            orderBy: request.OrderBy.SafeOrderBy("OccurredAtUtc"),
            sortDirection: request.SortDirection.SafeSortDirection(),
            pageNumber: pageNumber,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        var items = models.Select(m => new NearMissDto
        {
            NearMissId = m.NearMissId,
            TenantId = m.TenantId,
            SiteId = m.SiteId,
            Title = m.Title,
            Description = m.Description,
            OccurredAtUtc = m.OccurredAtUtc,
            PotentialSeverity = m.PotentialSeverity,
            HazardCategory = m.HazardCategory,
            Status = m.Status,
            ReportedByWorkerId = m.ReportedByWorkerId,
            LastModifiedAtUtc = m.LastModifiedAtUtc,
            ETag = m.ETag,
        }).ToList();

        return new PaginatedResult<NearMissDto>(items, total, pageNumber, pageSize);
    }
}
