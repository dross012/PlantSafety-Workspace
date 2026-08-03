// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-02
// FILE DESCRIPTION: Singleton state container for the Workflow module. Holds
//   the current actor's pending-approvals inbox and the currently loaded
//   WorkflowInstance detail (state + step history). Components never call
//   HttpClient directly — they dispatch to this state via WorkflowApiClient,
//   which is injected by the pages that trigger loads/actions.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.Workflow.Models;

namespace PlantSafety.UI.Modules.Workflow.State;

/// <summary>Singleton state container for the Workflow module.</summary>
public sealed class WorkflowState
{
    private IReadOnlyList<WorkflowInstanceDto> _pendingApprovals = [];
    private bool _isLoadingApprovals;
    private string? _approvalsError;
    private Guid? _actingInstanceId;
    private string? _actionError;

    private WorkflowInstanceDto? _instance;
    private IReadOnlyList<WorkflowStepHistoryEntryDto> _history = [];
    private bool _isLoadingInstance;
    private string? _instanceError;

    /// <summary>Gets the current actor's pending-approval WorkflowInstances.</summary>
    public IReadOnlyList<WorkflowInstanceDto> PendingApprovals => _pendingApprovals;

    /// <summary>Gets a value indicating whether the pending-approvals inbox is loading.</summary>
    public bool IsLoadingApprovals => _isLoadingApprovals;

    /// <summary>Gets a sanitized pending-approvals load error, or null.</summary>
    public string? ApprovalsError => _approvalsError;

    /// <summary>Gets the identity of the instance currently undergoing an approve/reject action, or null.</summary>
    public Guid? ActingInstanceId => _actingInstanceId;

    /// <summary>Gets a sanitized approve/reject action error, or null.</summary>
    public string? ActionError => _actionError;

    /// <summary>Gets the currently loaded WorkflowInstance detail record, or null.</summary>
    public WorkflowInstanceDto? Instance => _instance;

    /// <summary>Gets the currently loaded step execution history for <see cref="Instance"/>.</summary>
    public IReadOnlyList<WorkflowStepHistoryEntryDto> History => _history;

    /// <summary>Gets a value indicating whether the instance detail is loading.</summary>
    public bool IsLoadingInstance => _isLoadingInstance;

    /// <summary>Gets a sanitized instance load error, or null.</summary>
    public string? InstanceError => _instanceError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the pending-approvals inbox as loading.</summary>
    public void BeginApprovalsLoad() { _isLoadingApprovals = true; _approvalsError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded pending-approvals inbox.</summary>
    public void SetPendingApprovals(IReadOnlyList<WorkflowInstanceDto> items) { _pendingApprovals = items; _isLoadingApprovals = false; _approvalsError = null; NotifyStateChanged(); }

    /// <summary>Records a pending-approvals load failure.</summary>
    public void SetApprovalsError(string sanitizedError) { _approvalsError = sanitizedError; _isLoadingApprovals = false; NotifyStateChanged(); }

    /// <summary>Marks the given instance as undergoing an approve/reject/other action.</summary>
    public void BeginAction(Guid workflowInstanceId) { _actingInstanceId = workflowInstanceId; _actionError = null; NotifyStateChanged(); }

    /// <summary>Removes the acted-on instance from the pending-approvals inbox after a successful approve/reject.</summary>
    public void CompleteAction(Guid workflowInstanceId)
    {
        var updated = new List<WorkflowInstanceDto>(_pendingApprovals.Count);
        foreach (var item in _pendingApprovals)
        {
            if (item.WorkflowInstanceId != workflowInstanceId)
                updated.Add(item);
        }

        _pendingApprovals = updated;
        _actingInstanceId = null;
        _actionError = null;
        NotifyStateChanged();
    }

    /// <summary>Records an approve/reject/other action failure.</summary>
    public void SetActionError(string sanitizedError) { _actionError = sanitizedError; _actingInstanceId = null; NotifyStateChanged(); }

    /// <summary>Marks the instance detail panel as loading.</summary>
    public void BeginInstanceLoad() { _isLoadingInstance = true; _instanceError = null; NotifyStateChanged(); }

    /// <summary>Applies the freshly loaded instance detail and step history.</summary>
    public void SetInstance(WorkflowInstanceDto instance, IReadOnlyList<WorkflowStepHistoryEntryDto> history)
    {
        _instance = instance;
        _history = history;
        _isLoadingInstance = false;
        _instanceError = null;
        NotifyStateChanged();
    }

    /// <summary>Records an instance detail load failure.</summary>
    public void SetInstanceError(string sanitizedError) { _instanceError = sanitizedError; _isLoadingInstance = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
