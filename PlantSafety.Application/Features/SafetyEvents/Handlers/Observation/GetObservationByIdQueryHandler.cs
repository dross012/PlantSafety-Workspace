// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetObservationByIdQuery. Returns null when not found.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.SafetyEvents.Models.Observation;
using PlantSafety.Application.Features.SafetyEvents.Queries.Observation.Get;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.Observation;

/// <summary>Handles <see cref="GetObservationByIdQuery"/>.</summary>
public sealed class GetObservationByIdQueryHandler : IQueryHandler<GetObservationByIdQuery, ObservationDto?>
{
    private readonly IObservationRepository _repository;
    private readonly ILogger<GetObservationByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetObservationByIdQueryHandler(IObservationRepository repository, ILogger<GetObservationByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ObservationDto?> Handle(GetObservationByIdQuery request, CancellationToken cancellationToken)
    {
        var model = await _repository.GetByIdAsync(request.TenantId, request.ObservationId, cancellationToken);

        if (model is null)
        {
            _logger.LogDebug("Observation {ObservationId} not found for Tenant {TenantId}", request.ObservationId, request.TenantId);
            return null;
        }

        return new ObservationDto
        {
            ObservationId = model.ObservationId,
            TenantId = model.TenantId,
            SiteId = model.SiteId,
            Title = model.Title,
            Description = model.Description,
            ObservationType = model.ObservationType,
            ObservedAtUtc = model.ObservedAtUtc,
            ObservedByWorkerId = model.ObservedByWorkerId,
            Status = model.Status,
            LastModifiedAtUtc = model.LastModifiedAtUtc,
            ETag = model.ETag,
        };
    }
}
