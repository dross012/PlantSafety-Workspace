// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for ListEnvironmentalHealthQuery.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Extensions;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.EnvironmentalHealth.Models.Readings;
using PlantSafety.Application.Features.EnvironmentalHealth.Queries.Readings.ListReadings;

namespace PlantSafety.Application.Features.EnvironmentalHealth.Handlers.Readings;

/// <summary>Handles <see cref="ListEnvironmentalHealthQuery"/>. Returns paginated environmental health readings.</summary>
public sealed class ListEnvironmentalHealthQueryHandler : IQueryHandler<ListEnvironmentalHealthQuery, PaginatedResult<EnvironmentalHealthDto>>
{
    private readonly IEnvironmentalHealthReadingRepository _repository;
    private readonly ILogger<ListEnvironmentalHealthQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListEnvironmentalHealthQueryHandler(IEnvironmentalHealthReadingRepository repository, ILogger<ListEnvironmentalHealthQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PaginatedResult<EnvironmentalHealthDto>> Handle(ListEnvironmentalHealthQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber.ClampPageNumber();
        var pageSize = request.PageSize.ClampPageSize();

        var (models, total) = await _repository.ListAsync(
            tenantId: request.TenantId,
            siteId: request.SiteId,
            metricType: request.MetricType,
            status: request.Status,
            from: request.From,
            to: request.To,
            orderBy: request.OrderBy.SafeOrderBy("RecordedAtUtc"),
            sortDirection: request.SortDirection.SafeSortDirection(),
            pageNumber: pageNumber,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        var items = models.Select(m => new EnvironmentalHealthDto
        {
            EnvironmentalHealthId = m.EnvironmentalHealthId,
            TenantId = m.TenantId,
            SiteId = m.SiteId,
            MetricType = m.MetricType,
            MeasurementValue = m.MeasurementValue,
            Unit = m.Unit,
            Threshold = m.Threshold,
            Status = m.Status,
            RecordedAtUtc = m.RecordedAtUtc,
        }).ToList();

        return new PaginatedResult<EnvironmentalHealthDto>(items, total, pageNumber, pageSize);
    }
}
