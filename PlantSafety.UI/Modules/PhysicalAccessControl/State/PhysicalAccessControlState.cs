// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Singleton state container for the PhysicalAccessControl module. Holds data
//   for the Live Access &amp; Occupancy Map dashboard (a single safety zone's occupancy plus its
//   recent access events, looked up by zone ID — no list-all-zones read model exists), the
//   Anomaly Feed dashboard (active anomalies), and the Credential Lifecycle Status dashboard (a
//   single credential's status, looked up by credential ID). Components dispatch to this state
//   via PhysicalAccessControlApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.PhysicalAccessControl.Models;

namespace PlantSafety.UI.Modules.PhysicalAccessControl.State;

/// <summary>Singleton state container for the PhysicalAccessControl module dashboards.</summary>
public sealed class PhysicalAccessControlState
{
    // ── Live Access & Occupancy Map dashboard ───────────────────────────────
    private Guid? _occupancyZoneId;
    private SafetyZoneOccupancyDto? _zoneOccupancy;
    private IReadOnlyList<AccessEventDto> _zoneAccessEvents = [];
    private bool _isLoadingOccupancy;
    private string? _occupancyError;

    // ── Anomaly Feed dashboard ───────────────────────────────────────────────
    private IReadOnlyList<AccessAnomalyDto> _activeAnomalies = [];
    private bool _isLoadingAnomalies;
    private string? _anomaliesError;

    // ── Credential Lifecycle Status dashboard ───────────────────────────────
    private CredentialStatusDto? _lookedUpCredential;
    private string? _credentialLookupError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Gets the safety zone currently selected for the occupancy map, if any.</summary>
    public Guid? OccupancyZoneId => _occupancyZoneId;

    /// <summary>Gets the selected safety zone's current occupancy projection, or null.</summary>
    public SafetyZoneOccupancyDto? ZoneOccupancy => _zoneOccupancy;

    /// <summary>Gets the selected safety zone's recent access events.</summary>
    public IReadOnlyList<AccessEventDto> ZoneAccessEvents => _zoneAccessEvents;

    /// <summary>Gets a value indicating whether the occupancy map is loading.</summary>
    public bool IsLoadingOccupancy => _isLoadingOccupancy;

    /// <summary>Gets a sanitized occupancy load error, or null.</summary>
    public string? OccupancyError => _occupancyError;

    /// <summary>Gets the active (unresolved) access anomalies.</summary>
    public IReadOnlyList<AccessAnomalyDto> ActiveAnomalies => _activeAnomalies;

    /// <summary>Gets a value indicating whether the anomaly feed is loading.</summary>
    public bool IsLoadingAnomalies => _isLoadingAnomalies;

    /// <summary>Gets a sanitized anomaly feed load error, or null.</summary>
    public string? AnomaliesError => _anomaliesError;

    /// <summary>Gets the most recently looked-up credential, or null if none looked up / not found.</summary>
    public CredentialStatusDto? LookedUpCredential => _lookedUpCredential;

    /// <summary>Gets a sanitized credential-lookup error, or null.</summary>
    public string? CredentialLookupError => _credentialLookupError;

    /// <summary>Marks the occupancy map as loading for the given zone.</summary>
    /// <param name="zoneId">The safety zone identifier being viewed.</param>
    public void BeginOccupancyLoad(Guid zoneId) { _occupancyZoneId = zoneId; _isLoadingOccupancy = true; _occupancyError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded occupancy and access-event data for the selected zone.</summary>
    /// <param name="occupancy">The zone's current occupancy projection, or null if not found.</param>
    /// <param name="accessEvents">The zone's recent access events.</param>
    public void SetOccupancyData(SafetyZoneOccupancyDto? occupancy, IReadOnlyList<AccessEventDto> accessEvents)
    {
        _zoneOccupancy = occupancy;
        _zoneAccessEvents = accessEvents;
        _isLoadingOccupancy = false;
        _occupancyError = null;
        NotifyStateChanged();
    }

    /// <summary>Records an occupancy-map load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetOccupancyError(string sanitizedError) { _occupancyError = sanitizedError; _isLoadingOccupancy = false; NotifyStateChanged(); }

    /// <summary>Marks the anomaly feed as loading.</summary>
    public void BeginAnomaliesLoad() { _isLoadingAnomalies = true; _anomaliesError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded active-anomalies set.</summary>
    /// <param name="items">The full set of active anomaly records.</param>
    public void SetActiveAnomalies(IReadOnlyList<AccessAnomalyDto> items)
    {
        _activeAnomalies = items;
        _isLoadingAnomalies = false;
        _anomaliesError = null;
        NotifyStateChanged();
    }

    /// <summary>Records an anomaly-feed load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetAnomaliesError(string sanitizedError) { _anomaliesError = sanitizedError; _isLoadingAnomalies = false; NotifyStateChanged(); }

    /// <summary>Applies the result of an on-demand credential lookup.</summary>
    /// <param name="credential">The looked-up credential, or null if not found.</param>
    public void SetLookedUpCredential(CredentialStatusDto? credential) { _lookedUpCredential = credential; _credentialLookupError = null; NotifyStateChanged(); }

    /// <summary>Records a credential-lookup failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetCredentialLookupError(string sanitizedError) { _credentialLookupError = sanitizedError; _lookedUpCredential = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
