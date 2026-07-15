// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for RecordSustainabilityMetricCommand.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;
using PlantSafety.Application.Features.Sustainability.Commands.RecordMetric;

namespace PlantSafety.Application.Features.Sustainability.Handlers;

/// <summary>Handles RecordSustainabilityMetricCommand. Returns the new MetricId.</summary>
public sealed class RecordSustainabilityMetricCommandHandler : ICommandHandler<RecordSustainabilityMetricCommand, Guid>
{
    private readonly ISustainabilityRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<RecordSustainabilityMetricCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordSustainabilityMetricCommandHandler(
        ISustainabilityRepository repository,
        IDateTimeProvider clock,
        ILogger<RecordSustainabilityMetricCommandHandler> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(RecordSustainabilityMetricCommand request, CancellationToken cancellationToken)
    {
        var metricId = CorrelationIdGenerator.NewId(_clock.UtcNowOffset);

        await _repository.CreateAsync(
            metricId: metricId,
            tenantId: request.TenantId,
            siteId: request.SiteId,
            metricName: request.MetricName,
            category: request.Category,
            value: request.Value,
            unit: request.Unit,
            target: request.Target,
            periodEndUtc: request.PeriodEndUtc,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Sustainability metric {MetricId} recorded ({Category}) for Tenant {TenantId} by Actor {ActorId}",
            metricId, request.Category, request.TenantId, request.ActorId);

        return metricId;
    }
}
