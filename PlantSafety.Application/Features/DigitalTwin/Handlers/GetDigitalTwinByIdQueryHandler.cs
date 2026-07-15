// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for GetDigitalTwinByIdQuery. Returns null when not found.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.DigitalTwin.Models;
using PlantSafety.Application.Features.DigitalTwin.Queries.GetTwin;

namespace PlantSafety.Application.Features.DigitalTwin.Handlers;

/// <summary>Handles <see cref="GetDigitalTwinByIdQuery"/>.</summary>
public sealed class GetDigitalTwinByIdQueryHandler : IQueryHandler<GetDigitalTwinByIdQuery, DigitalTwinAssetDto?>
{
    private readonly IDigitalTwinRepository _repository;
    private readonly ILogger<GetDigitalTwinByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDigitalTwinByIdQueryHandler(IDigitalTwinRepository repository, ILogger<GetDigitalTwinByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<DigitalTwinAssetDto?> Handle(GetDigitalTwinByIdQuery request, CancellationToken cancellationToken)
    {
        var model = await _repository.GetByIdAsync(request.TenantId, request.TwinId, cancellationToken);

        if (model is null)
        {
            _logger.LogDebug("DigitalTwin {TwinId} not found for Tenant {TenantId}", request.TwinId, request.TenantId);
            return null;
        }

        return new DigitalTwinAssetDto
        {
            TwinId = model.TwinId,
            TenantId = model.TenantId,
            SiteId = model.SiteId,
            AssetName = model.AssetName,
            AssetType = model.AssetType,
            Status = model.Status,
            LastSyncAtUtc = model.LastSyncAtUtc,
            TelemetryHealthScore = model.TelemetryHealthScore,
        };
    }
}
