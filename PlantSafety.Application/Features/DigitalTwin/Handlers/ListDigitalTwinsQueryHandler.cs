// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for ListDigitalTwinsQuery.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.DigitalTwin.Models;
using PlantSafety.Application.Features.DigitalTwin.Queries.ListTwins;

namespace PlantSafety.Application.Features.DigitalTwin.Handlers;

/// <summary>Handles <see cref="ListDigitalTwinsQuery"/>. Returns the twin registry for a tenant.</summary>
public sealed class ListDigitalTwinsQueryHandler : IQueryHandler<ListDigitalTwinsQuery, IReadOnlyList<DigitalTwinAssetDto>>
{
    private readonly IDigitalTwinRepository _repository;
    private readonly ILogger<ListDigitalTwinsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListDigitalTwinsQueryHandler(IDigitalTwinRepository repository, ILogger<ListDigitalTwinsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DigitalTwinAssetDto>> Handle(ListDigitalTwinsQuery request, CancellationToken cancellationToken)
    {
        var models = await _repository.ListAsync(request.TenantId, request.SiteId, cancellationToken);

        return models.Select(m => new DigitalTwinAssetDto
        {
            TwinId = m.TwinId,
            TenantId = m.TenantId,
            SiteId = m.SiteId,
            AssetName = m.AssetName,
            AssetType = m.AssetType,
            Status = m.Status,
            LastSyncAtUtc = m.LastSyncAtUtc,
            TelemetryHealthScore = m.TelemetryHealthScore,
        }).ToList();
    }
}
