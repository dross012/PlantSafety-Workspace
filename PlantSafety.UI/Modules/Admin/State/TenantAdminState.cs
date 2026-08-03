// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-03
// FILE DESCRIPTION: Singleton state container for the Admin Portal's Tenant Management pages
//   (list, detail, provisioning wizard).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.Admin.Models;

namespace PlantSafety.UI.Modules.Admin.State;

/// <summary>Singleton state container for the Admin Portal's Tenant Management pages.</summary>
public sealed class TenantAdminState
{
    private IReadOnlyList<TenantAdminDto> _tenants = [];
    private TenantAdminDto? _selectedTenant;
    private bool _isLoadingList;
    private bool _isLoadingDetail;
    private bool _isBusy;
    private string? _listError;
    private string? _detailError;
    private string? _actionMessage;
    private TenantProvisioningResultDto? _lastProvisioningResult;
    private TenantDataExportResultDto? _lastExportResult;

    /// <summary>Gets every loaded tenant, most recently created first.</summary>
    public IReadOnlyList<TenantAdminDto> Tenants => _tenants;

    /// <summary>Gets the currently selected tenant detail, or null when none is selected.</summary>
    public TenantAdminDto? SelectedTenant => _selectedTenant;

    /// <summary>Gets a value indicating whether the tenant list is loading.</summary>
    public bool IsLoadingList => _isLoadingList;

    /// <summary>Gets a value indicating whether a tenant detail record is loading.</summary>
    public bool IsLoadingDetail => _isLoadingDetail;

    /// <summary>Gets a value indicating whether a lifecycle action (suspend/reactivate/archive/export/delete) is in flight.</summary>
    public bool IsBusy => _isBusy;

    /// <summary>Gets a sanitized list load error, or null.</summary>
    public string? ListError => _listError;

    /// <summary>Gets a sanitized detail load error, or null.</summary>
    public string? DetailError => _detailError;

    /// <summary>Gets the most recent action outcome message (success or sanitized error).</summary>
    public string? ActionMessage => _actionMessage;

    /// <summary>Gets the result of the most recently completed provisioning run, or null.</summary>
    public TenantProvisioningResultDto? LastProvisioningResult => _lastProvisioningResult;

    /// <summary>Gets the result of the most recently completed export, or null.</summary>
    public TenantDataExportResultDto? LastExportResult => _lastExportResult;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the tenant list as loading.</summary>
    public void BeginListLoad() { _isLoadingList = true; _listError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded tenant list.</summary>
    public void SetTenants(IReadOnlyList<TenantAdminDto> tenants)
    {
        _tenants = tenants;
        _isLoadingList = false;
        _listError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a tenant list load failure.</summary>
    public void SetListError(string sanitizedError) { _listError = sanitizedError; _isLoadingList = false; NotifyStateChanged(); }

    /// <summary>Marks the detail panel as loading.</summary>
    public void BeginDetailLoad() { _isLoadingDetail = true; _detailError = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded tenant detail record.</summary>
    public void SetSelectedTenant(TenantAdminDto tenant) { _selectedTenant = tenant; _isLoadingDetail = false; _detailError = null; NotifyStateChanged(); }

    /// <summary>Records a tenant detail load failure.</summary>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    /// <summary>Marks a lifecycle action as in flight.</summary>
    public void BeginAction() { _isBusy = true; _actionMessage = null; NotifyStateChanged(); }

    /// <summary>Records the outcome of a completed lifecycle action, updating the selected tenant in-place when supplied.</summary>
    public void CompleteAction(string message, TenantAdminDto? updatedTenant = null)
    {
        _isBusy = false;
        _actionMessage = message;
        if (updatedTenant is not null) _selectedTenant = updatedTenant;
        NotifyStateChanged();
    }

    /// <summary>Records a lifecycle action failure.</summary>
    public void SetActionError(string sanitizedError) { _isBusy = false; _actionMessage = sanitizedError; NotifyStateChanged(); }

    /// <summary>Applies the result of a completed provisioning run.</summary>
    public void SetProvisioningResult(TenantProvisioningResultDto result) { _lastProvisioningResult = result; _isBusy = false; _actionMessage = null; NotifyStateChanged(); }

    /// <summary>Applies the result of a completed export.</summary>
    public void SetExportResult(TenantDataExportResultDto result) { _lastExportResult = result; _isBusy = false; NotifyStateChanged(); }

    /// <summary>Clears the last provisioning result (e.g. when the wizard is reset).</summary>
    public void ClearProvisioningResult() { _lastProvisioningResult = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
