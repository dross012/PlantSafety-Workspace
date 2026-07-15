// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for GetFabricNodeByIdQuery. Returns null when not found.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.EnterpriseFabric.Models.FabricNodes;
using PlantSafety.Application.Features.EnterpriseFabric.Queries.FabricNodes.GetNode;

namespace PlantSafety.Application.Features.EnterpriseFabric.Handlers.FabricNodes;

/// <summary>Handles GetFabricNodeByIdQuery.</summary>
public sealed class GetFabricNodeByIdQueryHandler : IQueryHandler<GetFabricNodeByIdQuery, FabricNodeDto?>
{
    private readonly IFabricNodeRepository _repository;
    private readonly ILogger<GetFabricNodeByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetFabricNodeByIdQueryHandler(IFabricNodeRepository repository, ILogger<GetFabricNodeByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<FabricNodeDto?> Handle(GetFabricNodeByIdQuery request, CancellationToken cancellationToken)
    {
        var model = await _repository.GetByIdAsync(request.TenantId, request.NodeId, cancellationToken);

        if (model is null)
        {
            _logger.LogDebug("Fabric node {NodeId} not found for Tenant {TenantId}", request.NodeId, request.TenantId);
            return null;
        }

        return new FabricNodeDto
        {
            NodeId = model.NodeId,
            TenantId = model.TenantId,
            SiteId = model.SiteId,
            NodeName = model.NodeName,
            NodeType = model.NodeType,
            Status = model.Status,
            LastHeartbeatAtUtc = model.LastHeartbeatAtUtc,
            Region = model.Region,
        };
    }
}
