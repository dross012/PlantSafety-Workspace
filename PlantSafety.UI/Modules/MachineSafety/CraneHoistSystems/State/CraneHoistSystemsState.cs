// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Singleton state container for the MachineSafety.CraneHoistSystems
//   sub-aggregate. Holds data for the Crane/Hoist Fleet Status dashboard (full fleet plus
//   inspection-due emphasis) and the Operator Qualification Matrix dashboard (assets with
//   expiring qualifications), plus loading/error state. Components dispatch to this state via
//   CraneHoistSystemsApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.MachineSafety.CraneHoistSystems.Models;

namespace PlantSafety.UI.Modules.MachineSafety.CraneHoistSystems.State;

/// <summary>Singleton state container for the MachineSafety.CraneHoistSystems dashboards.</summary>
public sealed class CraneHoistSystemsState
{
    private IReadOnlyList<CraneHoistSystemsItemDto> _fleet = [];
    private IReadOnlyList<CraneHoistSystemsItemDto> _dueForInspection = [];
    private bool _isLoadingFleet;
    private string? _fleetError;

    private IReadOnlyList<CraneHoistSystemsItemDto> _expiringQualifications = [];
    private bool _isLoadingQualifications;
    private string? _qualificationsError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Gets the full crane/hoist fleet for the current tenant/site.</summary>
    public IReadOnlyList<CraneHoistSystemsItemDto> Fleet => _fleet;

    /// <summary>Gets the fleet assets due (or overdue) for inspection.</summary>
    public IReadOnlyList<CraneHoistSystemsItemDto> DueForInspection => _dueForInspection;

    /// <summary>Gets a value indicating whether the Crane/Hoist Fleet Status dashboard is loading.</summary>
    public bool IsLoadingFleet => _isLoadingFleet;

    /// <summary>Gets a sanitized Crane/Hoist Fleet Status load error, or null.</summary>
    public string? FleetError => _fleetError;

    /// <summary>Gets the fleet assets whose operator qualification expires within the configured window.</summary>
    public IReadOnlyList<CraneHoistSystemsItemDto> ExpiringQualifications => _expiringQualifications;

    /// <summary>Gets a value indicating whether the Operator Qualification Matrix is loading.</summary>
    public bool IsLoadingQualifications => _isLoadingQualifications;

    /// <summary>Gets a sanitized Operator Qualification Matrix load error, or null.</summary>
    public string? QualificationsError => _qualificationsError;

    /// <summary>Marks the Crane/Hoist Fleet Status dashboard as loading.</summary>
    public void BeginFleetLoad() { _isLoadingFleet = true; _fleetError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded fleet and due-for-inspection data.</summary>
    /// <param name="fleet">The full crane/hoist fleet.</param>
    /// <param name="dueForInspection">The fleet assets due (or overdue) for inspection.</param>
    public void SetFleetData(IReadOnlyList<CraneHoistSystemsItemDto> fleet, IReadOnlyList<CraneHoistSystemsItemDto> dueForInspection)
    {
        _fleet = fleet;
        _dueForInspection = dueForInspection;
        _isLoadingFleet = false;
        _fleetError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a Crane/Hoist Fleet Status load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetFleetError(string sanitizedError) { _fleetError = sanitizedError; _isLoadingFleet = false; NotifyStateChanged(); }

    /// <summary>Marks the Operator Qualification Matrix as loading.</summary>
    public void BeginQualificationsLoad() { _isLoadingQualifications = true; _qualificationsError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded expiring-qualifications set.</summary>
    /// <param name="items">The fleet assets whose operator qualification expires within the configured window.</param>
    public void SetExpiringQualifications(IReadOnlyList<CraneHoistSystemsItemDto> items) { _expiringQualifications = items; _isLoadingQualifications = false; _qualificationsError = null; NotifyStateChanged(); }

    /// <summary>Records an Operator Qualification Matrix load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetQualificationsError(string sanitizedError) { _qualificationsError = sanitizedError; _isLoadingQualifications = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
