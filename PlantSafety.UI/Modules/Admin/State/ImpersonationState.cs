// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-03
// FILE DESCRIPTION: Singleton state container for the platform-wide tenant-impersonation
//   banner (Phase 7 §6.3 Support Tooling). Holds the currently active session, if any, so the
//   persistent ImpersonationBanner component (rendered from MainLayout) can display it on
//   every page while a session is active, independent of which page started it.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

using PlantSafety.UI.Modules.Admin.Models;

namespace PlantSafety.UI.Modules.Admin.State;

/// <summary>Singleton state container for the platform-wide tenant-impersonation banner.</summary>
public sealed class ImpersonationState
{
    private ImpersonationSessionDto? _activeSession;
    private bool _isBusy;
    private string? _error;

    /// <summary>Gets the currently active impersonation session, or null when none is active.</summary>
    public ImpersonationSessionDto? ActiveSession => _activeSession;

    /// <summary>Gets a value indicating whether a start/end action is in flight.</summary>
    public bool IsBusy => _isBusy;

    /// <summary>Gets a sanitized error from the most recent start/end attempt, or null.</summary>
    public string? Error => _error;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks a start/end action as in flight.</summary>
    public void BeginAction() { _isBusy = true; _error = null; NotifyStateChanged(); }

    /// <summary>Applies a newly started (or refreshed) impersonation session.</summary>
    public void SetActiveSession(ImpersonationSessionDto session) { _activeSession = session; _isBusy = false; _error = null; NotifyStateChanged(); }

    /// <summary>Clears the active session (session ended, or expired).</summary>
    public void ClearActiveSession() { _activeSession = null; _isBusy = false; NotifyStateChanged(); }

    /// <summary>Records a start/end action failure.</summary>
    public void SetError(string sanitizedError) { _error = sanitizedError; _isBusy = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
