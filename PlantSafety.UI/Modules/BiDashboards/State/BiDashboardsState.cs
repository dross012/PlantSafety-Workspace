// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-02
// FILE DESCRIPTION: Singleton state container for the BI Dashboards page.
//   Holds independent loading/error/data state for each of the six
//   Phase 5 §4.3 sub-feature tabs (Scheduling, Production, Maintenance, Risk,
//   ContinuousImprovement, Audit) plus the shared date-range filter.
//   Components never call HttpClient directly — they dispatch to this state
//   via BiDashboardsApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.BiDashboards.Models;

namespace PlantSafety.UI.Modules.BiDashboards.State;

/// <summary>The tabs presented on the BI Dashboards page.</summary>
public enum BiDashboardsTab
{
    /// <summary>Scheduling BI dashboard tab.</summary>
    Scheduling,

    /// <summary>Production/Operational BI dashboard tab.</summary>
    Production,

    /// <summary>Maintenance BI dashboard tab.</summary>
    Maintenance,

    /// <summary>Risk BI dashboard tab.</summary>
    Risk,

    /// <summary>ContinuousImprovement BI dashboard tab.</summary>
    ContinuousImprovement,

    /// <summary>Audit BI dashboard tab.</summary>
    Audit
}

/// <summary>Singleton state container for the BI Dashboards page.</summary>
public sealed class BiDashboardsState
{
    private BiDashboardsTab _activeTab = BiDashboardsTab.Scheduling;

    private DateOnly _filterStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
    private DateOnly _filterEnd = DateOnly.FromDateTime(DateTime.UtcNow);

    private IReadOnlyList<SchedulingDashboardItemDto> _scheduling = [];
    private IReadOnlyList<ProductionDashboardItemDto> _production = [];
    private IReadOnlyList<MaintenanceDashboardItemDto> _maintenance = [];
    private IReadOnlyList<RiskDashboardItemDto> _risk = [];
    private IReadOnlyList<ContinuousImprovementDashboardItemDto> _continuousImprovement = [];
    private IReadOnlyList<AuditDashboardItemDto> _audit = [];

    private bool _isLoadingScheduling;
    private bool _isLoadingProduction;
    private bool _isLoadingMaintenance;
    private bool _isLoadingRisk;
    private bool _isLoadingContinuousImprovement;
    private bool _isLoadingAudit;

    private string? _schedulingError;
    private string? _productionError;
    private string? _maintenanceError;
    private string? _riskError;
    private string? _continuousImprovementError;
    private string? _auditError;

    /// <summary>Gets the currently active tab.</summary>
    public BiDashboardsTab ActiveTab => _activeTab;

    /// <summary>Gets the inclusive filter start date.</summary>
    public DateOnly FilterStart => _filterStart;

    /// <summary>Gets the inclusive filter end date.</summary>
    public DateOnly FilterEnd => _filterEnd;

    /// <summary>Gets the loaded Scheduling dashboard rows.</summary>
    public IReadOnlyList<SchedulingDashboardItemDto> Scheduling => _scheduling;

    /// <summary>Gets the loaded Production dashboard rows.</summary>
    public IReadOnlyList<ProductionDashboardItemDto> Production => _production;

    /// <summary>Gets the loaded Maintenance dashboard rows.</summary>
    public IReadOnlyList<MaintenanceDashboardItemDto> Maintenance => _maintenance;

    /// <summary>Gets the loaded Risk dashboard rows.</summary>
    public IReadOnlyList<RiskDashboardItemDto> Risk => _risk;

    /// <summary>Gets the loaded ContinuousImprovement dashboard rows.</summary>
    public IReadOnlyList<ContinuousImprovementDashboardItemDto> ContinuousImprovement => _continuousImprovement;

    /// <summary>Gets the loaded Audit dashboard rows.</summary>
    public IReadOnlyList<AuditDashboardItemDto> Audit => _audit;

    /// <summary>Gets a value indicating whether the Scheduling tab is loading.</summary>
    public bool IsLoadingScheduling => _isLoadingScheduling;

    /// <summary>Gets a value indicating whether the Production tab is loading.</summary>
    public bool IsLoadingProduction => _isLoadingProduction;

    /// <summary>Gets a value indicating whether the Maintenance tab is loading.</summary>
    public bool IsLoadingMaintenance => _isLoadingMaintenance;

    /// <summary>Gets a value indicating whether the Risk tab is loading.</summary>
    public bool IsLoadingRisk => _isLoadingRisk;

    /// <summary>Gets a value indicating whether the ContinuousImprovement tab is loading.</summary>
    public bool IsLoadingContinuousImprovement => _isLoadingContinuousImprovement;

    /// <summary>Gets a value indicating whether the Audit tab is loading.</summary>
    public bool IsLoadingAudit => _isLoadingAudit;

    /// <summary>Gets a sanitized Scheduling load error, or null.</summary>
    public string? SchedulingError => _schedulingError;

    /// <summary>Gets a sanitized Production load error, or null.</summary>
    public string? ProductionError => _productionError;

    /// <summary>Gets a sanitized Maintenance load error, or null.</summary>
    public string? MaintenanceError => _maintenanceError;

    /// <summary>Gets a sanitized Risk load error, or null.</summary>
    public string? RiskError => _riskError;

    /// <summary>Gets a sanitized ContinuousImprovement load error, or null.</summary>
    public string? ContinuousImprovementError => _continuousImprovementError;

    /// <summary>Gets a sanitized Audit load error, or null.</summary>
    public string? AuditError => _auditError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Switches the active tab.</summary>
    public void SetActiveTab(BiDashboardsTab tab) { _activeTab = tab; NotifyStateChanged(); }

    /// <summary>Applies a new date-range filter. Callers MUST reload the active tab afterward.</summary>
    public void SetFilter(DateOnly start, DateOnly end)
    {
        _filterStart = start;
        _filterEnd = end;
        _scheduling = [];
        _production = [];
        _maintenance = [];
        _risk = [];
        _continuousImprovement = [];
        _audit = [];
        NotifyStateChanged();
    }

    /// <summary>Marks the Scheduling tab as loading.</summary>
    public void BeginSchedulingLoad() { _isLoadingScheduling = true; _schedulingError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded Scheduling rows.</summary>
    public void SetScheduling(IReadOnlyList<SchedulingDashboardItemDto> items) { _scheduling = items; _isLoadingScheduling = false; _schedulingError = null; NotifyStateChanged(); }

    /// <summary>Records a Scheduling load failure.</summary>
    public void SetSchedulingError(string sanitizedError) { _schedulingError = sanitizedError; _isLoadingScheduling = false; NotifyStateChanged(); }

    /// <summary>Marks the Production tab as loading.</summary>
    public void BeginProductionLoad() { _isLoadingProduction = true; _productionError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded Production rows.</summary>
    public void SetProduction(IReadOnlyList<ProductionDashboardItemDto> items) { _production = items; _isLoadingProduction = false; _productionError = null; NotifyStateChanged(); }

    /// <summary>Records a Production load failure.</summary>
    public void SetProductionError(string sanitizedError) { _productionError = sanitizedError; _isLoadingProduction = false; NotifyStateChanged(); }

    /// <summary>Marks the Maintenance tab as loading.</summary>
    public void BeginMaintenanceLoad() { _isLoadingMaintenance = true; _maintenanceError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded Maintenance rows.</summary>
    public void SetMaintenance(IReadOnlyList<MaintenanceDashboardItemDto> items) { _maintenance = items; _isLoadingMaintenance = false; _maintenanceError = null; NotifyStateChanged(); }

    /// <summary>Records a Maintenance load failure.</summary>
    public void SetMaintenanceError(string sanitizedError) { _maintenanceError = sanitizedError; _isLoadingMaintenance = false; NotifyStateChanged(); }

    /// <summary>Marks the Risk tab as loading.</summary>
    public void BeginRiskLoad() { _isLoadingRisk = true; _riskError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded Risk rows.</summary>
    public void SetRisk(IReadOnlyList<RiskDashboardItemDto> items) { _risk = items; _isLoadingRisk = false; _riskError = null; NotifyStateChanged(); }

    /// <summary>Records a Risk load failure.</summary>
    public void SetRiskError(string sanitizedError) { _riskError = sanitizedError; _isLoadingRisk = false; NotifyStateChanged(); }

    /// <summary>Marks the ContinuousImprovement tab as loading.</summary>
    public void BeginContinuousImprovementLoad() { _isLoadingContinuousImprovement = true; _continuousImprovementError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded ContinuousImprovement rows.</summary>
    public void SetContinuousImprovement(IReadOnlyList<ContinuousImprovementDashboardItemDto> items) { _continuousImprovement = items; _isLoadingContinuousImprovement = false; _continuousImprovementError = null; NotifyStateChanged(); }

    /// <summary>Records a ContinuousImprovement load failure.</summary>
    public void SetContinuousImprovementError(string sanitizedError) { _continuousImprovementError = sanitizedError; _isLoadingContinuousImprovement = false; NotifyStateChanged(); }

    /// <summary>Marks the Audit tab as loading.</summary>
    public void BeginAuditLoad() { _isLoadingAudit = true; _auditError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded Audit rows.</summary>
    public void SetAudit(IReadOnlyList<AuditDashboardItemDto> items) { _audit = items; _isLoadingAudit = false; _auditError = null; NotifyStateChanged(); }

    /// <summary>Records an Audit load failure.</summary>
    public void SetAuditError(string sanitizedError) { _auditError = sanitizedError; _isLoadingAudit = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
