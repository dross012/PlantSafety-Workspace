// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-15
// FILE DESCRIPTION: Contract for the shared SignalR connection service used by the two
//   pages whose governing UI wireframe manuals mandate real-time push instead of client
//   polling (Production Tracking Dashboard — Volume T1; Incident SLA Monitor — Volume
//   ISM1). A single MSAL-authenticated hub connection is shared per app session; pages
//   subscribe to the lightweight "changed" broadcast for their module and re-fetch via
//   their existing typed ApiClient, exactly as a manual Refresh click already does.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Threading;
using System.Threading.Tasks;

namespace PlantSafety.UI.Infrastructure.Realtime;

/// <summary>Connection lifecycle state for the shared real-time hub connection.</summary>
public enum RealtimeConnectionStatus
{
    /// <summary>No connection attempt has been made yet, or the connection was explicitly stopped.</summary>
    Disconnected,

    /// <summary>A connection attempt is in progress.</summary>
    Connecting,

    /// <summary>The connection is established and group subscriptions are active.</summary>
    Connected,

    /// <summary>The connection dropped and the client is automatically retrying.</summary>
    Reconnecting,

    /// <summary>The connection could not be established or was permanently lost.
    /// Callers MUST fall back to their existing polling/manual-refresh path.</summary>
    Faulted,
}

/// <summary>
/// Shared, MSAL-authenticated SignalR connection to <c>PlantSafetyRealtimeHub</c>
/// (<c>/hubs/realtime</c>). Registered as a singleton so the connection is established
/// once per app session and reused by every real-time-capable page.
/// </summary>
/// <remarks>
/// Never throws out of <see cref="EnsureConnectedAsync"/> — connection failures are
/// surfaced via <see cref="Status"/>/<see cref="StatusChanged"/> so pages can degrade
/// gracefully (fallback polling, <c>ErrorBoundaryComponent</c>-compatible banner) instead
/// of crashing the render tree.
/// </remarks>
public interface IRealtimeConnectionService : IAsyncDisposable
{
    /// <summary>Gets the current connection lifecycle state.</summary>
    RealtimeConnectionStatus Status { get; }

    /// <summary>Raised whenever <see cref="Status"/> changes.</summary>
    event Action<RealtimeConnectionStatus>? StatusChanged;

    /// <summary>
    /// Ensures the shared hub connection is started. Safe to call from multiple pages
    /// concurrently — subsequent calls are no-ops while already connected/connecting.
    /// Never throws; failures are reflected via <see cref="Status"/> becoming
    /// <see cref="RealtimeConnectionStatus.Faulted"/>.
    /// </summary>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task EnsureConnectedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Joins the tenant-scoped Production Tracking Dashboard group (Volume T1) so the
    /// caller starts receiving <c>ProductionRunChanged</c> broadcasts. No-op (returns
    /// quietly) if the connection is not currently <see cref="RealtimeConnectionStatus.Connected"/>.
    /// </summary>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task JoinProductionTrackingGroupAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Joins the tenant-scoped Incident SLA Monitor group (Volume ISM1) so the caller
    /// starts receiving <c>IncidentSlaChanged</c> broadcasts. No-op (returns quietly) if
    /// the connection is not currently <see cref="RealtimeConnectionStatus.Connected"/>.
    /// </summary>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task JoinIncidentSlaGroupAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a handler for <c>ProductionRunChanged</c> broadcasts. The returned
    /// <see cref="IDisposable"/> MUST be disposed by the caller (typically in
    /// <c>IDisposable.Dispose</c>) to avoid leaking subscriptions across navigations.
    /// </summary>
    /// <param name="handler">Invoked on the UI thread is NOT guaranteed — callers must
    /// marshal back via <c>InvokeAsync</c> before touching Blazor state.</param>
    IDisposable OnProductionRunChanged(Action handler);

    /// <summary>
    /// Registers a handler for <c>IncidentSlaChanged</c> broadcasts. The returned
    /// <see cref="IDisposable"/> MUST be disposed by the caller to avoid leaking
    /// subscriptions across navigations.
    /// </summary>
    /// <param name="handler">Invoked on the UI thread is NOT guaranteed — callers must
    /// marshal back via <c>InvokeAsync</c> before touching Blazor state.</param>
    IDisposable OnIncidentSlaChanged(Action handler);
}
