// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Root singleton state container for the entire application
//   session. Aggregates initialization status, global error state, and
//   connectivity state. The AppShell component reads this to gate rendering
//   until the session is fully initialized.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.State;

/// <summary>Discriminated set of app-level initialization states.</summary>
public enum AppInitState
{
    /// <summary>App has not started initialization.</summary>
    NotStarted,

    /// <summary>App is loading configuration and authenticating.</summary>
    Initializing,

    /// <summary>App is fully initialized and ready to render module content.</summary>
    Ready,

    /// <summary>Initialization failed; error details are in <see cref="AppSessionState.InitError"/>.</summary>
    Failed
}

/// <summary>
/// Root singleton state container that gates the entire UI on successful initialization.
/// Injected into AppShell and top-level layout components.
/// </summary>
public sealed class AppSessionState
{
    private AppInitState _initState = AppInitState.NotStarted;
    private string? _initError;
    private bool _isOnline = true;

    /// <summary>Gets the current app initialization state.</summary>
    public AppInitState InitState => _initState;

    /// <summary>Gets a value indicating whether the app is fully initialized and ready.</summary>
    public bool IsReady => _initState == AppInitState.Ready;

    /// <summary>Gets a value indicating whether initialization is in progress.</summary>
    public bool IsInitializing => _initState == AppInitState.Initializing;

    /// <summary>Gets a sanitized initialization error message, or null when no error exists.</summary>
    public string? InitError => _initError;

    /// <summary>Gets a value indicating whether the device has network connectivity.</summary>
    public bool IsOnline => _isOnline;

    /// <summary>Raised on any state change; AppShell and layout components subscribe.</summary>
    public event Action? OnChange;

    /// <summary>Marks initialization as started.</summary>
    public void BeginInitialization()
    {
        _initState = AppInitState.Initializing;
        _initError = null;
        NotifyStateChanged();
    }

    /// <summary>Marks initialization as complete and the app as ready to render.</summary>
    public void MarkReady()
    {
        _initState = AppInitState.Ready;
        _initError = null;
        NotifyStateChanged();
    }

    /// <summary>
    /// Marks initialization as failed with a sanitized error message.
    /// </summary>
    /// <param name="sanitizedError">A safe error message free of PII, SQL, or stack traces.</param>
    public void MarkFailed(string sanitizedError)
    {
        _initState = AppInitState.Failed;
        _initError = sanitizedError;
        NotifyStateChanged();
    }

    /// <summary>Updates the network connectivity state.</summary>
    /// <param name="isOnline">True if the device is online; false if offline.</param>
    public void SetConnectivity(bool isOnline)
    {
        _isOnline = isOnline;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
