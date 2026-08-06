// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the ROI module. Holds the
//   currently computed ROI view model for the selected fiscal year, plus
//   loading/error/backend-unavailable state. Since no ROI HTTP endpoint
//   currently exists in PlantSafety.Api (see remarks on SafetyRoiDto), this
//   state exposes a dedicated BackendUnavailable flag so the dashboard page
//   can render a "needs backend wiring" notice (matching the established
//   AdminSetupPage pattern) instead of a hard error banner.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

using PlantSafety.UI.Modules.ROI.Models;

namespace PlantSafety.UI.Modules.ROI.State;

/// <summary>Singleton state container for the ROI module.</summary>
public sealed class RoiState
{
    private RoiViewModel? _roi;
    private bool _isLoading;
    private string? _error;
    private bool _backendUnavailable;
    private int _selectedFiscalYear = DateTime.UtcNow.Year;

    private bool _isCalculating;
    private string? _calculateError;
    private Guid? _calculatedRoiId;

    /// <summary>Gets the currently computed ROI, or null when not loaded.</summary>
    public RoiViewModel? Roi => _roi;

    /// <summary>Gets a value indicating whether the ROI calculation is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? Error => _error;

    /// <summary>Gets a value indicating whether the ROI endpoint is unavailable (no backend command/query wired yet).</summary>
    public bool BackendUnavailable => _backendUnavailable;

    /// <summary>Gets the currently selected fiscal year.</summary>
    public int SelectedFiscalYear => _selectedFiscalYear;

    /// <summary>Gets a value indicating whether a Calculate ROI submission is in flight.</summary>
    public bool IsCalculating => _isCalculating;

    /// <summary>Gets a sanitized Calculate ROI submission error, or null.</summary>
    public string? CalculateError => _calculateError;

    /// <summary>Gets the identifier of the most recently recorded ROI calculation, or null.</summary>
    public Guid? CalculatedRoiId => _calculatedRoiId;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Updates the selected fiscal year (triggers a re-fetch from the calling page).</summary>
    public void SetSelectedFiscalYear(int fiscalYear) { _selectedFiscalYear = fiscalYear; NotifyStateChanged(); }

    /// <summary>Marks the ROI calculation as loading.</summary>
    public void BeginLoad() { _isLoading = true; _error = null; _backendUnavailable = false; NotifyStateChanged(); }

    /// <summary>Applies the computed ROI view model.</summary>
    public void SetRoi(RoiViewModel roi) { _roi = roi; _isLoading = false; _error = null; _backendUnavailable = false; NotifyStateChanged(); }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _error = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Marks the ROI endpoint as unavailable (no backend wiring yet) rather than a hard error.</summary>
    public void SetBackendUnavailable() { _backendUnavailable = true; _isLoading = false; _roi = null; NotifyStateChanged(); }

    /// <summary>Marks a Calculate ROI submission as in flight.</summary>
    public void BeginCalculate() { _isCalculating = true; _calculateError = null; _calculatedRoiId = null; NotifyStateChanged(); }

    /// <summary>Records a successful Calculate ROI submission.</summary>
    public void SetCalculateSuccess(Guid roiCalculationId) { _calculatedRoiId = roiCalculationId; _isCalculating = false; _calculateError = null; NotifyStateChanged(); }

    /// <summary>Records a Calculate ROI submission failure with a sanitized message.</summary>
    public void SetCalculateError(string sanitizedError) { _calculateError = sanitizedError; _isCalculating = false; NotifyStateChanged(); }

    /// <summary>Resets Calculate ROI submission state (e.g. when navigating to the form).</summary>
    public void ResetCalculate() { _isCalculating = false; _calculateError = null; _calculatedRoiId = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
