// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for ListSustainabilityMetricsQuery.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.Sustainability.Models;
using PlantSafety.Application.Features.Sustainability.Queries.ListMetrics;

namespace PlantSafety.Application.Features.Sustainability.Handlers;

/// <summary>Handles ListSustainabilityMetricsQuery.</summary>
public sealed class ListSustainabilityMetricsQueryHandler : IQueryHandler<ListSustainabilityMetricsQuery, IReadOnlyList<SustainabilityMetricDto>>
{
    private readonly ISustainabilityRepository _repository;
    private readonly ILogger<ListSustainabilityMetricsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListSustainabilityMetricsQueryHandler(ISustainabilityRepository repository, ILogger<ListSustainabilityMetricsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SustainabilityMetricDto>> Handle(ListSustainabilityMetricsQuery request, CancellationToken cancellationToken)
    {
        var models = await _repository.ListAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

        return models.Select(m => new SustainabilityMetricDto
        {
            MetricId = m.MetricId,
            TenantId = m.TenantId,
            SiteId = m.SiteId,
            MetricName = m.MetricName,
            Category = m.Category,
            Value = m.Value,
            Unit = m.Unit,
            Target = m.Target,
            PeriodEndUtc = m.PeriodEndUtc,
            Status = m.Status,
        }).ToList();
    }
}
