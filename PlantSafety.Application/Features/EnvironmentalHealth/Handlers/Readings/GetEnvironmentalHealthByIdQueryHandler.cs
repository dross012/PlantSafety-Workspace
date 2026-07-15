// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for GetEnvironmentalHealthByIdQuery. Returns null when not found.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.EnvironmentalHealth.Models.Readings;
using PlantSafety.Application.Features.EnvironmentalHealth.Queries.Readings.GetReading;

namespace PlantSafety.Application.Features.EnvironmentalHealth.Handlers.Readings;

/// <summary>Handles <see cref="GetEnvironmentalHealthByIdQuery"/>.</summary>
public sealed class GetEnvironmentalHealthByIdQueryHandler : IQueryHandler<GetEnvironmentalHealthByIdQuery, EnvironmentalHealthDto?>
{
    private readonly IEnvironmentalHealthReadingRepository _repository;
    private readonly ILogger<GetEnvironmentalHealthByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEnvironmentalHealthByIdQueryHandler(IEnvironmentalHealthReadingRepository repository, ILogger<GetEnvironmentalHealthByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<EnvironmentalHealthDto?> Handle(GetEnvironmentalHealthByIdQuery request, CancellationToken cancellationToken)
    {
        var model = await _repository.GetByIdAsync(request.TenantId, request.EnvironmentalHealthId, cancellationToken);

        if (model is null)
        {
            _logger.LogDebug("EnvironmentalHealth {EnvironmentalHealthId} not found for Tenant {TenantId}", request.EnvironmentalHealthId, request.TenantId);
            return null;
        }

        return new EnvironmentalHealthDto
        {
            EnvironmentalHealthId = model.EnvironmentalHealthId,
            TenantId = model.TenantId,
            SiteId = model.SiteId,
            MetricType = model.MetricType,
            MeasurementValue = model.MeasurementValue,
            Unit = model.Unit,
            Threshold = model.Threshold,
            Status = model.Status,
            RecordedAtUtc = model.RecordedAtUtc,
        };
    }
}
