// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           GraphController.cs
//  Created:        2026-08-09
//  Description:    Write surface over Enterprise.Graph's real
//                  EnterpriseGraphBuilder (Phase D.1), replacing the current
//                  graph held by Enterprise.ApiGateway's
//                  IEnterpriseGraphProvider. Read-by-current-graph is already
//                  exposed via EnterpriseApiGatewayController's GET /graph
//                  (mounted as an ApplicationPart in Program.cs) -- this
//                  controller adds only what the gateway doesn't: building
//                  and publishing a new graph snapshot.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Enterprise.ApiGateway.Controllers;
using Enterprise.Graph.Abstractions.Enums;
using Enterprise.Graph.Abstractions.Graph;
using Enterprise.Graph.Abstractions.Ids;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>One node to add when building a new enterprise graph snapshot.</summary>
public sealed record GraphNodeRequest(Guid NodeId, EnterpriseNodeType NodeType, Guid SourceId, string Label);

/// <summary>One directed edge to add when building a new enterprise graph snapshot.</summary>
public sealed record GraphEdgeRequest(Guid FromNodeId, Guid ToNodeId, EnterpriseEdgeType EdgeType);

/// <summary>The full request body for (re)building the current enterprise graph snapshot.</summary>
public sealed record BuildGraphRequest(IReadOnlyList<GraphNodeRequest> Nodes, IReadOnlyList<GraphEdgeRequest> Edges);

/// <summary>Exposes Enterprise.Graph's builder to PlantSafety.UI, publishing the result via IEnterpriseGraphProvider.</summary>
[ApiController]
[Route("api/v1/ui/graph")]
public sealed class GraphController(InMemoryEnterpriseGraphProvider graphProvider) : ControllerBase
{
    /// <summary>POST api/v1/ui/graph -- deterministically (re)builds and publishes the current enterprise graph snapshot.</summary>
    [HttpPost]
    public ActionResult<EnterpriseGraph> Build([FromBody] BuildGraphRequest request)
    {
        DateTime now = DateTime.UtcNow;
        var builder = new EnterpriseGraphBuilder();

        foreach (GraphNodeRequest node in request.Nodes)
        {
            builder.AddNode(EnterpriseNodeId.From(node.NodeId), node.NodeType, node.SourceId, node.Label, now);
        }

        foreach (GraphEdgeRequest edge in request.Edges)
        {
            builder.AddEdge(EnterpriseNodeId.From(edge.FromNodeId), EnterpriseNodeId.From(edge.ToNodeId), edge.EdgeType, now);
        }

        EnterpriseGraph graph = builder.Build();
        graphProvider.SetCurrentGraph(graph);
        return Ok(graph);
    }
}
