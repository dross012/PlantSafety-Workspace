// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Singleton state container for the EnvironmentalHealth.WaterDischargeCompliance
//   sub-module. Holds data for the Discharge Compliance Status dashboard (discharge points, open
//   exceedances, DMR due/submitted status, and an on-demand permit-expiration lookup) and the
//   Effluent Trend Charts by discharge point dashboard, plus loading/error state. Components
//   dispatch to this state via WaterDischargeComplianceApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.EnvironmentalHealth.WaterDischargeCompliance.Models;

namespace PlantSafety.UI.Modules.EnvironmentalHealth.WaterDischargeCompliance.State;

/// <summary>Singleton state container for the EnvironmentalHealth.WaterDischargeCompliance sub-module dashboards.</summary>
public sealed class WaterDischargeComplianceState
{
    // ── Discharge Compliance Status dashboard ───────────────────────────────
    private IReadOnlyList<DischargePointDto> _dischargePoints = [];
    private IReadOnlyList<LimitExceedanceDto> _activeExceedances = [];
    private IReadOnlyList<DischargeMonitoringReportDto> _dmrReports = [];
    private DischargePermitDto? _lookedUpPermit;
    private bool _isLoadingComplianceStatus;
    private string? _complianceStatusError;
    private string? _permitLookupError;

    // ── Effluent Trend Charts dashboard ─────────────────────────────────────
    private Guid? _trendDischargePointId;
    private IReadOnlyList<EffluentSampleDto> _sampleHistory = [];
    private IReadOnlyList<DischargeLimitDto> _limitsForTrendPoint = [];
    private bool _isLoadingTrend;
    private string? _trendError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Gets the discharge points for the current tenant/site.</summary>
    public IReadOnlyList<DischargePointDto> DischargePoints => _dischargePoints;

    /// <summary>Gets the active (open) discharge limit exceedances.</summary>
    public IReadOnlyList<LimitExceedanceDto> ActiveExceedances => _activeExceedances;

    /// <summary>Gets the Discharge Monitoring Reports overlapping the current reporting period.</summary>
    public IReadOnlyList<DischargeMonitoringReportDto> DmrReports => _dmrReports;

    /// <summary>Gets the most recently looked-up discharge permit, or null if none looked up / not found.</summary>
    public DischargePermitDto? LookedUpPermit => _lookedUpPermit;

    /// <summary>Gets a value indicating whether the Discharge Compliance Status dashboard is loading.</summary>
    public bool IsLoadingComplianceStatus => _isLoadingComplianceStatus;

    /// <summary>Gets a sanitized Discharge Compliance Status load error, or null.</summary>
    public string? ComplianceStatusError => _complianceStatusError;

    /// <summary>Gets a sanitized permit-lookup error, or null.</summary>
    public string? PermitLookupError => _permitLookupError;

    /// <summary>Gets the discharge point currently selected for the Effluent Trend Chart, if any.</summary>
    public Guid? TrendDischargePointId => _trendDischargePointId;

    /// <summary>Gets the effluent sample history for the selected discharge point.</summary>
    public IReadOnlyList<EffluentSampleDto> SampleHistory => _sampleHistory;

    /// <summary>Gets the discharge limits defined for the selected discharge point (trend reference lines).</summary>
    public IReadOnlyList<DischargeLimitDto> LimitsForTrendPoint => _limitsForTrendPoint;

    /// <summary>Gets a value indicating whether the Effluent Trend Chart is loading.</summary>
    public bool IsLoadingTrend => _isLoadingTrend;

    /// <summary>Gets a sanitized Effluent Trend Chart load error, or null.</summary>
    public string? TrendError => _trendError;

    /// <summary>Marks the Discharge Compliance Status dashboard as loading.</summary>
    public void BeginComplianceStatusLoad() { _isLoadingComplianceStatus = true; _complianceStatusError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded discharge-points, exceedance, and DMR data.</summary>
    /// <param name="dischargePoints">The discharge points for the current tenant/site.</param>
    /// <param name="activeExceedances">The active discharge limit exceedances.</param>
    /// <param name="dmrReports">The Discharge Monitoring Reports overlapping the current period.</param>
    public void SetComplianceStatusData(
        IReadOnlyList<DischargePointDto> dischargePoints,
        IReadOnlyList<LimitExceedanceDto> activeExceedances,
        IReadOnlyList<DischargeMonitoringReportDto> dmrReports)
    {
        _dischargePoints = dischargePoints;
        _activeExceedances = activeExceedances;
        _dmrReports = dmrReports;
        _isLoadingComplianceStatus = false;
        _complianceStatusError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a Discharge Compliance Status load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetComplianceStatusError(string sanitizedError) { _complianceStatusError = sanitizedError; _isLoadingComplianceStatus = false; NotifyStateChanged(); }

    /// <summary>Applies the result of an on-demand discharge-permit lookup.</summary>
    /// <param name="permit">The looked-up permit, or null if not found.</param>
    public void SetLookedUpPermit(DischargePermitDto? permit) { _lookedUpPermit = permit; _permitLookupError = null; NotifyStateChanged(); }

    /// <summary>Records a permit-lookup failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetPermitLookupError(string sanitizedError) { _permitLookupError = sanitizedError; _lookedUpPermit = null; NotifyStateChanged(); }

    /// <summary>Marks the Effluent Trend Chart as loading for the given discharge point.</summary>
    /// <param name="dischargePointId">The discharge point identifier being charted.</param>
    public void BeginTrendLoad(Guid dischargePointId) { _trendDischargePointId = dischargePointId; _isLoadingTrend = true; _trendError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded sample-history and limit data for the Effluent Trend Chart.</summary>
    /// <param name="sampleHistory">The effluent sample history for the selected discharge point.</param>
    /// <param name="limits">The discharge limits defined for the selected discharge point.</param>
    public void SetTrendData(IReadOnlyList<EffluentSampleDto> sampleHistory, IReadOnlyList<DischargeLimitDto> limits)
    {
        _sampleHistory = sampleHistory;
        _limitsForTrendPoint = limits;
        _isLoadingTrend = false;
        _trendError = null;
        NotifyStateChanged();
    }

    /// <summary>Records an Effluent Trend Chart load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetTrendError(string sanitizedError) { _trendError = sanitizedError; _isLoadingTrend = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
