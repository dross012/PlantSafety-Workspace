// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Singleton state container for the LogisticsTransportSafety module. Holds
//   data for the Fleet Safety Status dashboard (carriers), Active Trip Tracker dashboard (active
//   transport trips), and Transport Incident Trends dashboard (incident history), plus
//   loading/error state. Components dispatch to this state via LogisticsTransportSafetyApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.LogisticsTransportSafety.Models;

namespace PlantSafety.UI.Modules.LogisticsTransportSafety.State;

/// <summary>Singleton state container for the LogisticsTransportSafety module dashboards.</summary>
public sealed class LogisticsTransportSafetyState
{
    private IReadOnlyList<CarrierDto> _carriers = [];
    private bool _isLoadingCarriers;
    private string? _carriersError;

    private IReadOnlyList<TransportTripDto> _activeTrips = [];
    private bool _isLoadingTrips;
    private string? _tripsError;

    private IReadOnlyList<TransportIncidentDto> _incidents = [];
    private bool _isLoadingIncidents;
    private string? _incidentsError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Gets the carrier fleet records for the current tenant/site.</summary>
    public IReadOnlyList<CarrierDto> Carriers => _carriers;

    /// <summary>Gets a value indicating whether the Fleet Safety Status dashboard is loading.</summary>
    public bool IsLoadingCarriers => _isLoadingCarriers;

    /// <summary>Gets a sanitized Fleet Safety Status load error, or null.</summary>
    public string? CarriersError => _carriersError;

    /// <summary>Gets the currently active transport trips.</summary>
    public IReadOnlyList<TransportTripDto> ActiveTrips => _activeTrips;

    /// <summary>Gets a value indicating whether the Active Trip Tracker is loading.</summary>
    public bool IsLoadingTrips => _isLoadingTrips;

    /// <summary>Gets a sanitized Active Trip Tracker load error, or null.</summary>
    public string? TripsError => _tripsError;

    /// <summary>Gets the transport incident history.</summary>
    public IReadOnlyList<TransportIncidentDto> Incidents => _incidents;

    /// <summary>Gets a value indicating whether the Transport Incident Trends dashboard is loading.</summary>
    public bool IsLoadingIncidents => _isLoadingIncidents;

    /// <summary>Gets a sanitized Transport Incident Trends load error, or null.</summary>
    public string? IncidentsError => _incidentsError;

    /// <summary>Marks the Fleet Safety Status dashboard as loading.</summary>
    public void BeginCarriersLoad() { _isLoadingCarriers = true; _carriersError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded carrier set.</summary>
    /// <param name="items">The full set of carrier records.</param>
    public void SetCarriers(IReadOnlyList<CarrierDto> items) { _carriers = items; _isLoadingCarriers = false; _carriersError = null; NotifyStateChanged(); }

    /// <summary>Records a Fleet Safety Status load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetCarriersError(string sanitizedError) { _carriersError = sanitizedError; _isLoadingCarriers = false; NotifyStateChanged(); }

    /// <summary>Marks the Active Trip Tracker as loading.</summary>
    public void BeginTripsLoad() { _isLoadingTrips = true; _tripsError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded active-trips set.</summary>
    /// <param name="items">The full set of active trip records.</param>
    public void SetActiveTrips(IReadOnlyList<TransportTripDto> items) { _activeTrips = items; _isLoadingTrips = false; _tripsError = null; NotifyStateChanged(); }

    /// <summary>Records an Active Trip Tracker load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetTripsError(string sanitizedError) { _tripsError = sanitizedError; _isLoadingTrips = false; NotifyStateChanged(); }

    /// <summary>Marks the Transport Incident Trends dashboard as loading.</summary>
    public void BeginIncidentsLoad() { _isLoadingIncidents = true; _incidentsError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded incident-history set.</summary>
    /// <param name="items">The full set of transport incident records.</param>
    public void SetIncidents(IReadOnlyList<TransportIncidentDto> items) { _incidents = items; _isLoadingIncidents = false; _incidentsError = null; NotifyStateChanged(); }

    /// <summary>Records a Transport Incident Trends load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetIncidentsError(string sanitizedError) { _incidentsError = sanitizedError; _isLoadingIncidents = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
