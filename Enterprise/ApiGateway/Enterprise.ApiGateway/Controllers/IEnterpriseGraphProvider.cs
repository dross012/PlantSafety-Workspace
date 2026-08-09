// ============================================================================
//  Project:        Enterprise.ApiGateway
//  Module:         Controllers
//  File:           IEnterpriseGraphProvider.cs
//  Created:        2026-08-08
//  Description:    Phase D.3: the gateway's access point to the current
//                  EnterpriseGraph. Enterprise.Graph (Phase D.1) is
//                  deliberately just an immutable value type plus a
//                  builder -- it owns no service/repository layer of its
//                  own -- so the gateway needs a minimal seam of its own to
//                  read whatever graph the composition root has built.
//                  InMemoryEnterpriseGraphProvider is a trivial,
//                  thread-safe holder; a real host wires a fresher provider
//                  in if/when one is built.
// ============================================================================
using Enterprise.Graph.Abstractions.Graph;

namespace Enterprise.ApiGateway.Controllers;

/// <summary>Provides read access to the current, unified <see cref="EnterpriseGraph"/>.</summary>
public interface IEnterpriseGraphProvider
{
    /// <summary>Returns the current enterprise graph.</summary>
    EnterpriseGraph GetCurrentGraph();
}

/// <summary>Thread-safe, in-memory, process-local <see cref="IEnterpriseGraphProvider"/> holding a settable current graph.</summary>
public sealed class InMemoryEnterpriseGraphProvider : IEnterpriseGraphProvider
{
    private readonly object _lock = new();
    private EnterpriseGraph _currentGraph = new([], []);

    /// <inheritdoc />
    public EnterpriseGraph GetCurrentGraph()
    {
        lock (_lock)
        {
            return _currentGraph;
        }
    }

    /// <summary>Replaces the current graph wholesale.</summary>
    public void SetCurrentGraph(EnterpriseGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);
        lock (_lock)
        {
            _currentGraph = graph;
        }
    }
}
