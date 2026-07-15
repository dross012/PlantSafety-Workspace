// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for GetSustainabilityMetricByIdQuery. Returns null when not found.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.Sustainability.Models;
using PlantSafety.Application.Features.Sustainability.Queries.GetMetric;

namespace PlantSafety.Application.Features.Sustainability.Handlers;

/// <summary>Handles GetSustainabilityMetricByIdQuery.</summary>
public sealed class GetSustainabilityMetricByIdQueryHandler : IQueryHandler<GetSustainabilityMetricByIdQuery, SustainabilityMetricDto?>
{
    private readonly ISustainabilityRepository _repository;
    private readonly ILogger<GetSustainabilityMetricByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSustainabilityMetricByIdQueryHandler(ISustainabilityRepository repository, ILogger<GetSustainabilityMetricByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SustainabilityMetricDto?> Handle(GetSustainabilityMetricByIdQuery request, CancellationToken cancellationToken)
    {
        var model = await _repository.GetByIdAsync(request.TenantId, request.MetricId, cancellationToken);

        if (model is null)
        {
            _logger.LogDebug("Sustainability metric {MetricId} not found for Tenant {TenantId}", request.MetricId, request.TenantId);
            return null;
        }

        return new SustainabilityMetricDto
        {
            MetricId = model.MetricId,
            TenantId = model.TenantId,
            SiteId = model.SiteId,
            MetricName = model.MetricName,
            Category = model.Category,
            Value = model.Value,
            Unit = model.Unit,
            Target = model.Target,
            PeriodEndUtc = model.PeriodEndUtc,
            Status = model.Status,
        };
    }
}
