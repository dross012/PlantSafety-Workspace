// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-04
// FILE DESCRIPTION: Singleton state container for the Local Emergency Coordination module. Holds
//   the current mutual aid agreement registry, active joint response plans, and recent joint
//   debrief records, plus loading/error state.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.LocalEmergencyCoordination.Models;

namespace PlantSafety.UI.Modules.LocalEmergencyCoordination.State;

/// <summary>Singleton state container for the Local Emergency Coordination module.</summary>
public sealed class LocalEmergencyCoordinationState
{
    private IReadOnlyList<MutualAidAgreementDto> _agreements = [];
    private IReadOnlyList<JointDebriefDto> _debriefs = [];
    private bool _isLoading;
    private string? _error;

    /// <summary>Gets the current mutual aid agreement registry. Never null; empty when not loaded.</summary>
    public IReadOnlyList<MutualAidAgreementDto> Agreements => _agreements;

    /// <summary>Gets the count of agreements currently pending renewal or expired -- backs the agreement currency KPI.</summary>
    public int AgreementsNeedingAttentionCount => _agreements.Count(a =>
        string.Equals(a.RenewalStatus, "PendingRenewal", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(a.RenewalStatus, "Expired", StringComparison.OrdinalIgnoreCase));

    /// <summary>Gets recent joint post-incident debrief records.</summary>
    public IReadOnlyList<JointDebriefDto> Debriefs => _debriefs;

    /// <summary>Gets a value indicating whether the dashboard is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? Error => _error;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the dashboard as loading.</summary>
    public void BeginLoad() { _isLoading = true; _error = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded mutual aid agreement registry.</summary>
    public void SetAgreements(IReadOnlyList<MutualAidAgreementDto> agreements) { _agreements = agreements; _isLoading = false; _error = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded joint debrief records.</summary>
    public void SetDebriefs(IReadOnlyList<JointDebriefDto> debriefs) { _debriefs = debriefs; NotifyStateChanged(); }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _error = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
