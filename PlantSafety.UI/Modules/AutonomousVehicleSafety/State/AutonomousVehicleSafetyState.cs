// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Singleton state container for the AutonomousVehicleSafety module. Holds data
//   for the Autonomous Fleet Status &amp; Geofence Map dashboard (active geofence violations plus an
//   on-demand asset lookup), the Safety Case Currency Board dashboard (looked up by asset ID — no
//   list-all read model exists), and the Human-Robot Interaction Heatmap dashboard (interaction
//   events over a period), plus loading/error state. Components dispatch to this state via
//   AutonomousVehicleSafetyApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.AutonomousVehicleSafety.Models;

namespace PlantSafety.UI.Modules.AutonomousVehicleSafety.State;

/// <summary>Singleton state container for the AutonomousVehicleSafety module dashboards.</summary>
public sealed class AutonomousVehicleSafetyState
{
    private IReadOnlyList<OperatingGeofenceDto> _geofenceViolations = [];
    private bool _isLoadingGeofences;
    private string? _geofencesError;
    private AutonomousVehicleAssetDto? _lookedUpAsset;
    private string? _assetLookupError;

    private SafetyCaseValidationDto? _safetyCase;
    private Guid? _safetyCaseAssetId;
    private string? _safetyCaseError;

    private IReadOnlyList<HumanRobotInteractionEventDto> _interactionEvents = [];
    private bool _isLoadingInteractions;
    private string? _interactionsError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Gets the geofences currently reporting active violations.</summary>
    public IReadOnlyList<OperatingGeofenceDto> GeofenceViolations => _geofenceViolations;

    /// <summary>Gets a value indicating whether the geofence violations list is loading.</summary>
    public bool IsLoadingGeofences => _isLoadingGeofences;

    /// <summary>Gets a sanitized geofence-violations load error, or null.</summary>
    public string? GeofencesError => _geofencesError;

    /// <summary>Gets the most recently looked-up autonomous vehicle asset, or null.</summary>
    public AutonomousVehicleAssetDto? LookedUpAsset => _lookedUpAsset;

    /// <summary>Gets a sanitized asset-lookup error, or null.</summary>
    public string? AssetLookupError => _assetLookupError;

    /// <summary>Gets the asset currently selected for the Safety Case Currency Board, if any.</summary>
    public Guid? SafetyCaseAssetId => _safetyCaseAssetId;

    /// <summary>Gets the looked-up safety-case validation status, or null.</summary>
    public SafetyCaseValidationDto? SafetyCase => _safetyCase;

    /// <summary>Gets a sanitized Safety Case Currency Board lookup error, or null.</summary>
    public string? SafetyCaseError => _safetyCaseError;

    /// <summary>Gets the human-robot interaction events for the selected period.</summary>
    public IReadOnlyList<HumanRobotInteractionEventDto> InteractionEvents => _interactionEvents;

    /// <summary>Gets a value indicating whether the Human-Robot Interaction Heatmap is loading.</summary>
    public bool IsLoadingInteractions => _isLoadingInteractions;

    /// <summary>Gets a sanitized Human-Robot Interaction Heatmap load error, or null.</summary>
    public string? InteractionsError => _interactionsError;

    /// <summary>Marks the geofence-violations list as loading.</summary>
    public void BeginGeofencesLoad() { _isLoadingGeofences = true; _geofencesError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded geofence-violations set.</summary>
    /// <param name="items">The full set of geofences reporting active violations.</param>
    public void SetGeofenceViolations(IReadOnlyList<OperatingGeofenceDto> items) { _geofenceViolations = items; _isLoadingGeofences = false; _geofencesError = null; NotifyStateChanged(); }

    /// <summary>Records a geofence-violations load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetGeofencesError(string sanitizedError) { _geofencesError = sanitizedError; _isLoadingGeofences = false; NotifyStateChanged(); }

    /// <summary>Applies the result of an on-demand fleet-asset lookup.</summary>
    /// <param name="asset">The looked-up asset, or null if not found.</param>
    public void SetLookedUpAsset(AutonomousVehicleAssetDto? asset) { _lookedUpAsset = asset; _assetLookupError = null; NotifyStateChanged(); }

    /// <summary>Records a fleet-asset lookup failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetAssetLookupError(string sanitizedError) { _assetLookupError = sanitizedError; _lookedUpAsset = null; NotifyStateChanged(); }

    /// <summary>Applies the result of an on-demand safety-case status lookup.</summary>
    /// <param name="assetId">The asset identifier looked up.</param>
    /// <param name="safetyCase">The looked-up safety-case validation status, or null if not found.</param>
    public void SetSafetyCase(Guid assetId, SafetyCaseValidationDto? safetyCase) { _safetyCaseAssetId = assetId; _safetyCase = safetyCase; _safetyCaseError = null; NotifyStateChanged(); }

    /// <summary>Records a safety-case lookup failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetSafetyCaseError(string sanitizedError) { _safetyCaseError = sanitizedError; _safetyCase = null; NotifyStateChanged(); }

    /// <summary>Marks the interaction-events list as loading.</summary>
    public void BeginInteractionsLoad() { _isLoadingInteractions = true; _interactionsError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded interaction-events set.</summary>
    /// <param name="items">The full set of interaction event records for the selected period.</param>
    public void SetInteractionEvents(IReadOnlyList<HumanRobotInteractionEventDto> items) { _interactionEvents = items; _isLoadingInteractions = false; _interactionsError = null; NotifyStateChanged(); }

    /// <summary>Records a Human-Robot Interaction Heatmap load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetInteractionsError(string sanitizedError) { _interactionsError = sanitizedError; _isLoadingInteractions = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
