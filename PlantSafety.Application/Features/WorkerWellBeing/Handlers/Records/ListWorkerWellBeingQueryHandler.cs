// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for ListWorkerWellBeingQuery.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Extensions;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.WorkerWellBeing.Models.Records;
using PlantSafety.Application.Features.WorkerWellBeing.Queries.Records.ListRecords;

namespace PlantSafety.Application.Features.WorkerWellBeing.Handlers.Records;

/// <summary>Handles <see cref="ListWorkerWellBeingQuery"/>. Returns paginated worker wellbeing records.</summary>
public sealed class ListWorkerWellBeingQueryHandler : IQueryHandler<ListWorkerWellBeingQuery, PaginatedResult<WorkerWellBeingDto>>
{
    private readonly IWorkerWellBeingRecordRepository _repository;
    private readonly ILogger<ListWorkerWellBeingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListWorkerWellBeingQueryHandler(IWorkerWellBeingRecordRepository repository, ILogger<ListWorkerWellBeingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PaginatedResult<WorkerWellBeingDto>> Handle(ListWorkerWellBeingQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber.ClampPageNumber();
        var pageSize = request.PageSize.ClampPageSize();

        var (models, total) = await _repository.ListAsync(
            tenantId: request.TenantId,
            workerId: request.WorkerId,
            wellBeingRiskLevel: request.WellBeingRiskLevel,
            status: request.Status,
            orderBy: request.OrderBy.SafeOrderBy("RecordedAtUtc"),
            sortDirection: request.SortDirection.SafeSortDirection(),
            pageNumber: pageNumber,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        var items = models.Select(m => new WorkerWellBeingDto
        {
            WorkerWellBeingId = m.WorkerWellBeingId,
            TenantId = m.TenantId,
            WorkerId = m.WorkerId,
            WellBeingRiskLevel = m.WellBeingRiskLevel,
            Status = m.Status,
            RecordedAtUtc = m.RecordedAtUtc,
        }).ToList();

        return new PaginatedResult<WorkerWellBeingDto>(items, total, pageNumber, pageSize);
    }
}
