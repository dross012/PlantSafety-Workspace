// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Scorecards page. Holds
//   independent loading/error/data state for each of the four data-backed
//   tabs (Compliance, Department, Supervisor, Training) plus the shared
//   date-range filter. Components never call HttpClient directly — they
//   dispatch to this state via ScorecardsApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.Scorecards.Models;

namespace PlantSafety.UI.Modules.Scorecards.State;

/// <summary>The tabs presented on the Scorecards page.</summary>
public enum ScorecardsTab
{
    /// <summary>Compliance scorecard tab.</summary>
    Compliance,

    /// <summary>Department scorecard tab.</summary>
    Department,

    /// <summary>Supervisor scorecard tab.</summary>
    Supervisor,

    /// <summary>Training scorecard tab.</summary>
    Training
}

/// <summary>Singleton state container for the Scorecards page.</summary>
public sealed class ScorecardsState
{
    private ScorecardsTab _activeTab = ScorecardsTab.Compliance;

    private DateOnly _filterStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90));
    private DateOnly _filterEnd = DateOnly.FromDateTime(DateTime.UtcNow);

    private IReadOnlyList<ComplianceScorecardDto> _compliance = [];
    private IReadOnlyList<DepartmentScorecardDto> _department = [];
    private IReadOnlyList<SupervisorScorecardDto> _supervisor = [];
    private IReadOnlyList<TrainingScorecardDto> _training = [];

    private bool _isLoadingCompliance;
    private bool _isLoadingDepartment;
    private bool _isLoadingSupervisor;
    private bool _isLoadingTraining;

    private string? _complianceError;
    private string? _departmentError;
    private string? _supervisorError;
    private string? _trainingError;

    /// <summary>Gets the currently active tab.</summary>
    public ScorecardsTab ActiveTab => _activeTab;

    /// <summary>Gets the inclusive filter start date.</summary>
    public DateOnly FilterStart => _filterStart;

    /// <summary>Gets the inclusive filter end date.</summary>
    public DateOnly FilterEnd => _filterEnd;

    /// <summary>Gets the loaded compliance scorecard entries.</summary>
    public IReadOnlyList<ComplianceScorecardDto> Compliance => _compliance;

    /// <summary>Gets the loaded department scorecard entries.</summary>
    public IReadOnlyList<DepartmentScorecardDto> Department => _department;

    /// <summary>Gets the loaded supervisor scorecard entries.</summary>
    public IReadOnlyList<SupervisorScorecardDto> Supervisor => _supervisor;

    /// <summary>Gets the loaded training scorecard entries.</summary>
    public IReadOnlyList<TrainingScorecardDto> Training => _training;

    /// <summary>Gets a value indicating whether the Compliance tab is loading.</summary>
    public bool IsLoadingCompliance => _isLoadingCompliance;

    /// <summary>Gets a value indicating whether the Department tab is loading.</summary>
    public bool IsLoadingDepartment => _isLoadingDepartment;

    /// <summary>Gets a value indicating whether the Supervisor tab is loading.</summary>
    public bool IsLoadingSupervisor => _isLoadingSupervisor;

    /// <summary>Gets a value indicating whether the Training tab is loading.</summary>
    public bool IsLoadingTraining => _isLoadingTraining;

    /// <summary>Gets a sanitized Compliance load error, or null.</summary>
    public string? ComplianceError => _complianceError;

    /// <summary>Gets a sanitized Department load error, or null.</summary>
    public string? DepartmentError => _departmentError;

    /// <summary>Gets a sanitized Supervisor load error, or null.</summary>
    public string? SupervisorError => _supervisorError;

    /// <summary>Gets a sanitized Training load error, or null.</summary>
    public string? TrainingError => _trainingError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Switches the active tab.</summary>
    public void SetActiveTab(ScorecardsTab tab) { _activeTab = tab; NotifyStateChanged(); }

    /// <summary>Applies a new date-range filter. Callers MUST reload the active tab afterward.</summary>
    public void SetFilter(DateOnly start, DateOnly end)
    {
        _filterStart = start;
        _filterEnd = end;
        _compliance = [];
        _department = [];
        _supervisor = [];
        _training = [];
        NotifyStateChanged();
    }

    /// <summary>Marks the Compliance tab as loading.</summary>
    public void BeginComplianceLoad() { _isLoadingCompliance = true; _complianceError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded compliance scorecard entries.</summary>
    public void SetCompliance(IReadOnlyList<ComplianceScorecardDto> items) { _compliance = items; _isLoadingCompliance = false; _complianceError = null; NotifyStateChanged(); }

    /// <summary>Records a Compliance load failure.</summary>
    public void SetComplianceError(string sanitizedError) { _complianceError = sanitizedError; _isLoadingCompliance = false; NotifyStateChanged(); }

    /// <summary>Marks the Department tab as loading.</summary>
    public void BeginDepartmentLoad() { _isLoadingDepartment = true; _departmentError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded department scorecard entries.</summary>
    public void SetDepartment(IReadOnlyList<DepartmentScorecardDto> items) { _department = items; _isLoadingDepartment = false; _departmentError = null; NotifyStateChanged(); }

    /// <summary>Records a Department load failure.</summary>
    public void SetDepartmentError(string sanitizedError) { _departmentError = sanitizedError; _isLoadingDepartment = false; NotifyStateChanged(); }

    /// <summary>Marks the Supervisor tab as loading.</summary>
    public void BeginSupervisorLoad() { _isLoadingSupervisor = true; _supervisorError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded supervisor scorecard entries.</summary>
    public void SetSupervisor(IReadOnlyList<SupervisorScorecardDto> items) { _supervisor = items; _isLoadingSupervisor = false; _supervisorError = null; NotifyStateChanged(); }

    /// <summary>Records a Supervisor load failure.</summary>
    public void SetSupervisorError(string sanitizedError) { _supervisorError = sanitizedError; _isLoadingSupervisor = false; NotifyStateChanged(); }

    /// <summary>Marks the Training tab as loading.</summary>
    public void BeginTrainingLoad() { _isLoadingTraining = true; _trainingError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded training scorecard entries.</summary>
    public void SetTraining(IReadOnlyList<TrainingScorecardDto> items) { _training = items; _isLoadingTraining = false; _trainingError = null; NotifyStateChanged(); }

    /// <summary>Records a Training load failure.</summary>
    public void SetTrainingError(string sanitizedError) { _trainingError = sanitizedError; _isLoadingTraining = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
