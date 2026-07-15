// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Enterprise Fabric
//   module. Holds the loaded fabric node registry, the selected node for the
//   detail drilldown, and loading/error state. Components dispatch to this
//   state via EnterpriseFabricApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.EnterpriseFabric.Models;

namespace PlantSafety.UI.Modules.EnterpriseFabric.State;

/// <summary>Singleton state container for the Enterprise Fabric module.</summary>
public sealed class EnterpriseFabricState
{
    private IReadOnlyList<FabricNodeDto> _nodes = [];
    private FabricNodeDto? _selected;
    private bool _isLoading;
    private string? _loadError;

    /// <summary>Gets the currently loaded fabric nodes. Never null; empty when not loaded.</summary>
    public IReadOnlyList<FabricNodeDto> Nodes => _nodes;

    /// <summary>Gets the currently selected node for the detail view, or null.</summary>
    public FabricNodeDto? Selected => _selected;

    /// <summary>Gets a value indicating whether the registry is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? LoadError => _loadError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the registry as loading.</summary>
    public void BeginLoad() { _isLoading = true; _loadError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded set of fabric nodes.</summary>
    /// <param name="nodes">The node items returned by the API.</param>
    public void SetNodes(IReadOnlyList<FabricNodeDto> nodes)
    {
        _nodes = nodes;
        _isLoading = false;
        _loadError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetLoadError(string sanitizedError) { _loadError = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Selects a node by identifier from the currently loaded set, if present.</summary>
    /// <param name="nodeId">The node identifier to select.</param>
    /// <returns>True if the node was found and selected.</returns>
    public bool SelectById(Guid nodeId)
    {
        _selected = _nodes.FirstOrDefault(n => n.NodeId == nodeId);
        NotifyStateChanged();
        return _selected is not null;
    }

    /// <summary>Clears the selected node.</summary>
    public void ClearSelection() { _selected = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
