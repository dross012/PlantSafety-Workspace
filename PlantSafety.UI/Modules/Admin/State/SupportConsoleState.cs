// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-03
// FILE DESCRIPTION: Singleton state container for the Admin Portal's Support Console page
//   (Phase 7 §6.3 Support Tooling): the selected tenant's audit stream viewer.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.Admin.Models;

namespace PlantSafety.UI.Modules.Admin.State;

/// <summary>Singleton state container for the Admin Portal's Support Console page.</summary>
public sealed class SupportConsoleState
{
    private Guid? _selectedTenantId;
    private IReadOnlyList<PermissionUsageLogEntryDto> _auditEntries = [];
    private bool _isLoadingAuditStream;
    private string? _auditStreamError;

    /// <summary>Gets the tenant currently selected for the audit stream viewer, or null.</summary>
    public Guid? SelectedTenantId => _selectedTenantId;

    /// <summary>Gets the loaded audit stream entries for the selected tenant, newest first.</summary>
    public IReadOnlyList<PermissionUsageLogEntryDto> AuditEntries => _auditEntries;

    /// <summary>Gets a value indicating whether the audit stream is loading.</summary>
    public bool IsLoadingAuditStream => _isLoadingAuditStream;

    /// <summary>Gets a sanitized audit stream load error, or null.</summary>
    public string? AuditStreamError => _auditStreamError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Selects a tenant and marks the audit stream as loading.</summary>
    public void SelectTenant(Guid tenantId) { _selectedTenantId = tenantId; _isLoadingAuditStream = true; _auditStreamError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded audit stream entries.</summary>
    public void SetAuditEntries(IReadOnlyList<PermissionUsageLogEntryDto> entries)
    {
        _auditEntries = entries;
        _isLoadingAuditStream = false;
        _auditStreamError = null;
        NotifyStateChanged();
    }

    /// <summary>Records an audit stream load failure.</summary>
    public void SetAuditStreamError(string sanitizedError) { _auditStreamError = sanitizedError; _isLoadingAuditStream = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
