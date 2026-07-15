// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for ListProcessesQuery.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Extensions;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.ProcessManagement.Models.Processes;
using PlantSafety.Application.Features.ProcessManagement.Queries.Processes.ListProcesses;

namespace PlantSafety.Application.Features.ProcessManagement.Handlers.Processes;

/// <summary>Handles <see cref="ListProcessesQuery"/>. Returns paginated process definition records.</summary>
public sealed class ListProcessesQueryHandler : IQueryHandler<ListProcessesQuery, PaginatedResult<ProcessDefinitionDto>>
{
    private readonly IProcessRepository _repository;
    private readonly ILogger<ListProcessesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListProcessesQueryHandler(IProcessRepository repository, ILogger<ListProcessesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PaginatedResult<ProcessDefinitionDto>> Handle(ListProcessesQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber.ClampPageNumber();
        var pageSize = request.PageSize.ClampPageSize();

        var (models, total) = await _repository.ListAsync(
            tenantId: request.TenantId,
            siteId: request.SiteId,
            category: request.Category,
            status: request.Status,
            orderBy: request.OrderBy.SafeOrderBy("ProcessName"),
            sortDirection: request.SortDirection.SafeSortDirection(),
            pageNumber: pageNumber,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        var items = models.Select(m => new ProcessDefinitionDto
        {
            ProcessId = m.ProcessId,
            TenantId = m.TenantId,
            SiteId = m.SiteId,
            ProcessName = m.ProcessName,
            Category = m.Category,
            Status = m.Status,
            Version = m.Version,
            OwnerId = m.OwnerId,
            LastModifiedAtUtc = m.LastModifiedAtUtc,
            ETag = m.ETag,
        }).ToList();

        return new PaginatedResult<ProcessDefinitionDto>(items, total, pageNumber, pageSize);
    }
}
