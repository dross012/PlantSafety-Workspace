// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for ListPermitsQuery.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Extensions;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.Compliance.Models;
using PlantSafety.Application.Features.Compliance.Queries.Permits.List;

namespace PlantSafety.Application.Features.Compliance.Handlers.Permits;

/// <summary>Handles <see cref="ListPermitsQuery"/>. Returns paginated permit records.</summary>
public sealed class ListPermitsQueryHandler : IQueryHandler<ListPermitsQuery, PaginatedResult<PermitDto>>
{
    private readonly IPermitRepository _repository;
    private readonly ILogger<ListPermitsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListPermitsQueryHandler(IPermitRepository repository, ILogger<ListPermitsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PaginatedResult<PermitDto>> Handle(ListPermitsQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber.ClampPageNumber();
        var pageSize = request.PageSize.ClampPageSize();

        var (models, total) = await _repository.ListAsync(
            tenantId: request.TenantId,
            siteId: request.SiteId,
            permitType: request.PermitType,
            status: request.Status,
            validFromUtc: request.ValidFromUtc,
            validToUtc: request.ValidToUtc,
            orderBy: request.OrderBy.SafeOrderBy(defaultField: "ValidFromUtc"),
            sortDirection: request.SortDirection.SafeSortDirection(),
            pageNumber: pageNumber,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        var items = models.Select(m => new PermitDto
        {
            PermitId = m.PermitId,
            TenantId = m.TenantId,
            SiteId = m.SiteId,
            PermitType = m.PermitType,
            PermitNumber = m.PermitNumber,
            WorkDescription = m.WorkDescription,
            WorkLocation = m.WorkLocation,
            WorkLeadId = m.WorkLeadId,
            ValidFromUtc = m.ValidFromUtc,
            ValidToUtc = m.ValidToUtc,
            Status = m.Status,
            ApprovedByActorId = m.ApprovedByActorId,
            ApprovedAtUtc = m.ApprovedAtUtc,
            LastModifiedAtUtc = m.LastModifiedAtUtc,
            ETag = m.ETag,
        }).ToList();

        return new PaginatedResult<PermitDto>(items, total, pageNumber, pageSize);
    }
}
