// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Login page (Volume AUTH1).
//   Tracks the in-progress sign-in interaction only — the resulting identity and
//   tenant are written to UserSessionState / TenantState on success, never here.
//   Trimmed from the AUTH1 reference AuthState shape (no username/password/mfaCode
//   fields) because PlantSafety's Zero-Trust authentication model delegates the
//   entire credential exchange to Azure AD / Entra ID via MSAL — the UI never
//   collects or stores a password or MFA code itself.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.State;

/// <summary>Discriminated set of last-known Login page actions, included in observability events.</summary>
public enum AuthLastAction
{
    /// <summary>No sign-in action has been attempted yet in this app session.</summary>
    None,

    /// <summary>Interactive Entra ID sign-in was initiated.</summary>
    SignIn,

    /// <summary>Interactive Entra ID sign-in completed successfully.</summary>
    SignInSucceeded,

    /// <summary>Interactive Entra ID sign-in failed or was cancelled.</summary>
    SignInFailed
}

/// <summary>
/// Singleton state container for the Login page. Immutable snapshot semantics:
/// every mutator replaces the relevant fields and raises <see cref="OnChange"/>.
/// </summary>
public sealed class AuthState
{
    private bool _isAuthenticating;
    private string? _errorMessage;
    private AuthLastAction _lastAction = AuthLastAction.None;

    /// <summary>Gets a value indicating whether an interactive sign-in is currently in progress.</summary>
    public bool IsAuthenticating => _isAuthenticating;

    /// <summary>Gets the sanitized, security-safe error message for the last failed attempt, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Gets the last sign-in action taken, included in observability events.</summary>
    public AuthLastAction LastAction => _lastAction;

    /// <summary>Raised whenever the state changes; the Login page subscribes to re-render.</summary>
    public event Action? OnChange;

    /// <summary>Marks an interactive sign-in attempt as started. Clears any prior error.</summary>
    public void BeginSignIn()
    {
        _isAuthenticating = true;
        _errorMessage = null;
        _lastAction = AuthLastAction.SignIn;
        NotifyStateChanged();
    }

    /// <summary>Marks the interactive sign-in attempt as successfully completed.</summary>
    public void SignInSucceeded()
    {
        _isAuthenticating = false;
        _errorMessage = null;
        _lastAction = AuthLastAction.SignInSucceeded;
        NotifyStateChanged();
    }

    /// <summary>
    /// Records a sign-in failure with a security-safe message. Never includes tokens,
    /// passwords, or raw exception details (per AUTH1 §Observability).
    /// </summary>
    /// <param name="sanitizedError">A safe, user-facing error message.</param>
    public void SignInFailed(string sanitizedError)
    {
        _isAuthenticating = false;
        _errorMessage = sanitizedError;
        _lastAction = AuthLastAction.SignInFailed;
        NotifyStateChanged();
    }

    /// <summary>Resets the state to its initial values (e.g., after navigating away from Login).</summary>
    public void Reset()
    {
        _isAuthenticating = false;
        _errorMessage = null;
        _lastAction = AuthLastAction.None;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
