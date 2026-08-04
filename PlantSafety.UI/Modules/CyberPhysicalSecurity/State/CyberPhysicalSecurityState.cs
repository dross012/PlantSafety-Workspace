// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Singleton state container for the CyberPhysicalSecurity module. Holds data
//   for the OT Asset Security Status dashboard (looked up by asset ID — no list-all-assets read
//   model exists), the Control-Change Audit Feed dashboard (unauthorized control-change events),
//   and the Active Sabotage Indicator Board dashboard (active indicators), plus loading/error
//   state. Components dispatch to this state via CyberPhysicalSecurityApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.CyberPhysicalSecurity.Models;

namespace PlantSafety.UI.Modules.CyberPhysicalSecurity.State;

/// <summary>Singleton state container for the CyberPhysicalSecurity module dashboards.</summary>
public sealed class CyberPhysicalSecurityState
{
    private OtAssetDto? _lookedUpAsset;
    private string? _assetLookupError;

    private IReadOnlyList<ControlChangeEventDto> _unauthorizedChanges = [];
    private bool _isLoadingChanges;
    private string? _changesError;

    private IReadOnlyList<SabotageIndicatorDto> _activeIndicators = [];
    private bool _isLoadingIndicators;
    private string? _indicatorsError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Gets the most recently looked-up OT asset, or null if none looked up / not found.</summary>
    public OtAssetDto? LookedUpAsset => _lookedUpAsset;

    /// <summary>Gets a sanitized OT asset lookup error, or null.</summary>
    public string? AssetLookupError => _assetLookupError;

    /// <summary>Gets the control-change events currently flagged unauthorized.</summary>
    public IReadOnlyList<ControlChangeEventDto> UnauthorizedChanges => _unauthorizedChanges;

    /// <summary>Gets a value indicating whether the Control-Change Audit Feed is loading.</summary>
    public bool IsLoadingChanges => _isLoadingChanges;

    /// <summary>Gets a sanitized Control-Change Audit Feed load error, or null.</summary>
    public string? ChangesError => _changesError;

    /// <summary>Gets the currently active sabotage indicators.</summary>
    public IReadOnlyList<SabotageIndicatorDto> ActiveIndicators => _activeIndicators;

    /// <summary>Gets a value indicating whether the Active Sabotage Indicator Board is loading.</summary>
    public bool IsLoadingIndicators => _isLoadingIndicators;

    /// <summary>Gets a sanitized Active Sabotage Indicator Board load error, or null.</summary>
    public string? IndicatorsError => _indicatorsError;

    /// <summary>Applies the result of an on-demand OT asset lookup.</summary>
    /// <param name="asset">The looked-up asset, or null if not found.</param>
    public void SetLookedUpAsset(OtAssetDto? asset) { _lookedUpAsset = asset; _assetLookupError = null; NotifyStateChanged(); }

    /// <summary>Records an OT asset lookup failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetAssetLookupError(string sanitizedError) { _assetLookupError = sanitizedError; _lookedUpAsset = null; NotifyStateChanged(); }

    /// <summary>Marks the Control-Change Audit Feed as loading.</summary>
    public void BeginChangesLoad() { _isLoadingChanges = true; _changesError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded unauthorized-changes set.</summary>
    /// <param name="items">The full set of unauthorized control-change events.</param>
    public void SetUnauthorizedChanges(IReadOnlyList<ControlChangeEventDto> items) { _unauthorizedChanges = items; _isLoadingChanges = false; _changesError = null; NotifyStateChanged(); }

    /// <summary>Records a Control-Change Audit Feed load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetChangesError(string sanitizedError) { _changesError = sanitizedError; _isLoadingChanges = false; NotifyStateChanged(); }

    /// <summary>Marks the Active Sabotage Indicator Board as loading.</summary>
    public void BeginIndicatorsLoad() { _isLoadingIndicators = true; _indicatorsError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded active-indicators set.</summary>
    /// <param name="items">The full set of active sabotage indicator records.</param>
    public void SetActiveIndicators(IReadOnlyList<SabotageIndicatorDto> items) { _activeIndicators = items; _isLoadingIndicators = false; _indicatorsError = null; NotifyStateChanged(); }

    /// <summary>Records an Active Sabotage Indicator Board load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetIndicatorsError(string sanitizedError) { _indicatorsError = sanitizedError; _isLoadingIndicators = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
