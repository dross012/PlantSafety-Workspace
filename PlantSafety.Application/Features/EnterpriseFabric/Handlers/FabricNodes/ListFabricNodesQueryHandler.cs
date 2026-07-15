// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for ListFabricNodesQuery.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.EnterpriseFabric.Models.FabricNodes;
using PlantSafety.Application.Features.EnterpriseFabric.Queries.FabricNodes.ListNodes;

namespace PlantSafety.Application.Features.EnterpriseFabric.Handlers.FabricNodes;

/// <summary>Handles ListFabricNodesQuery.</summary>
public sealed class ListFabricNodesQueryHandler : IQueryHandler<ListFabricNodesQuery, IReadOnlyList<FabricNodeDto>>
{
    private readonly IFabricNodeRepository _repository;
    private readonly ILogger<ListFabricNodesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListFabricNodesQueryHandler(IFabricNodeRepository repository, ILogger<ListFabricNodesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FabricNodeDto>> Handle(ListFabricNodesQuery request, CancellationToken cancellationToken)
    {
        var models = await _repository.ListAsync(request.TenantId, request.SiteId, cancellationToken);

        return models.Select(m => new FabricNodeDto
        {
            NodeId = m.NodeId,
            TenantId = m.TenantId,
            SiteId = m.SiteId,
            NodeName = m.NodeName,
            NodeType = m.NodeType,
            Status = m.Status,
            LastHeartbeatAtUtc = m.LastHeartbeatAtUtc,
            Region = m.Region,
        }).ToList();
    }
}
