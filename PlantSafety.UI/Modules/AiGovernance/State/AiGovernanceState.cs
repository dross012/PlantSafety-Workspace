// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the AI Governance module.
//   Holds the loaded governed model registry, the selected model for the
//   detail drilldown, and loading/error state. Components dispatch to this
//   state via AiGovernanceApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.AiGovernance.Models;

namespace PlantSafety.UI.Modules.AiGovernance.State;

/// <summary>Singleton state container for the AI Governance module.</summary>
public sealed class AiGovernanceState
{
    private IReadOnlyList<GovernedModelDto> _models = [];
    private GovernedModelDto? _selected;
    private bool _isLoading;
    private string? _loadError;

    /// <summary>Gets the currently loaded governed models. Never null; empty when not loaded.</summary>
    public IReadOnlyList<GovernedModelDto> Models => _models;

    /// <summary>Gets the currently selected model for the detail view, or null.</summary>
    public GovernedModelDto? Selected => _selected;

    /// <summary>Gets a value indicating whether the registry is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? LoadError => _loadError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the registry as loading.</summary>
    public void BeginLoad() { _isLoading = true; _loadError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded set of governed models.</summary>
    /// <param name="models">The model items returned by the API.</param>
    public void SetModels(IReadOnlyList<GovernedModelDto> models)
    {
        _models = models;
        _isLoading = false;
        _loadError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetLoadError(string sanitizedError) { _loadError = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Selects a model by identifier from the currently loaded set, if present.</summary>
    /// <param name="modelId">The model identifier to select.</param>
    /// <returns>True if the model was found and selected.</returns>
    public bool SelectById(Guid modelId)
    {
        _selected = _models.FirstOrDefault(m => m.ModelId == modelId);
        NotifyStateChanged();
        return _selected is not null;
    }

    /// <summary>Clears the selected model.</summary>
    public void ClearSelection() { _selected = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
