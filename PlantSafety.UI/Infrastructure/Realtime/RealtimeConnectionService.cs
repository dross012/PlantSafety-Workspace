// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-15
// FILE DESCRIPTION: Singleton implementation of IRealtimeConnectionService. Owns a
//   single MSAL-authenticated SignalR HubConnection to PlantSafetyRealtimeHub
//   (/hubs/realtime) for the lifetime of the app session, with automatic reconnection.
//   Group membership (Production Tracking / Incident SLA) is re-joined transparently
//   whenever the underlying connection reconnects, so pages never need to know about
//   reconnection plumbing — they just call JoinXGroupAsync once and register handlers.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.Auth;

namespace PlantSafety.UI.Infrastructure.Realtime;

/// <summary>
/// Client method names invoked by <c>PlantSafetyRealtimeHub</c> — must match
/// <c>PlantSafety.Api.Realtime.SignalRRealtimeNotifier</c> exactly.
/// </summary>
internal static class RealtimeClientMethods
{
    public const string ProductionRunChanged = "ProductionRunChanged";
    public const string IncidentSlaChanged = "IncidentSlaChanged";
}

/// <summary>
/// Client-side group-join hub method names — must match
/// <c>PlantSafety.Api.Realtime.PlantSafetyRealtimeHub</c> exactly.
/// </summary>
internal static class RealtimeHubMethods
{
    public const string JoinProductionTrackingGroup = "JoinProductionTrackingGroupAsync";
    public const string JoinIncidentSlaGroup = "JoinIncidentSlaGroupAsync";
}

/// <inheritdoc cref="IRealtimeConnectionService"/>
public sealed class RealtimeConnectionService : IRealtimeConnectionService
{
    private readonly HubConnection _connection;
    private readonly ILogger<RealtimeConnectionService> _logger;
    private readonly SemaphoreSlim _startLock = new(1, 1);

    private bool _productionTrackingGroupRequested;
    private bool _incidentSlaGroupRequested;
    private RealtimeConnectionStatus _status = RealtimeConnectionStatus.Disconnected;

    /// <inheritdoc/>
    public RealtimeConnectionStatus Status => _status;

    /// <inheritdoc/>
    public event Action<RealtimeConnectionStatus>? StatusChanged;

    /// <summary>Initializes the shared hub connection (not yet started — see <see cref="EnsureConnectedAsync"/>).</summary>
    /// <param name="configuration">Application configuration providing Api:BaseUrl.</param>
    /// <param name="tokenProvider">MSAL token provider used to authenticate the hub connection.</param>
    /// <param name="logger">Structured logger; tokens are never logged.</param>
    public RealtimeConnectionService(
        IConfiguration configuration,
        IMsalTokenProvider tokenProvider,
        ILogger<RealtimeConnectionService> logger)
    {
        _logger = logger;

        var baseUrl = configuration["Api:BaseUrl"]
            ?? throw new InvalidOperationException("Api:BaseUrl configuration is missing.");

        // The hub is mapped at the web host root ("/hubs/realtime"), not under the
        // versioned "/api/" REST surface — resolve against the authority only.
        var hubUri = new Uri(new Uri(baseUrl), "/hubs/realtime");

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUri, options =>
            {
                options.AccessTokenProvider = async () =>
                    await tokenProvider.AcquireTokenAsync().ConfigureAwait(false);
            })
            .WithAutomaticReconnect(new[]
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30),
            })
            .Build();

        _connection.Reconnecting += OnReconnectingAsync;
        _connection.Reconnected += OnReconnectedAsync;
        _connection.Closed += OnClosedAsync;
    }

    /// <inheritdoc/>
    public async Task EnsureConnectedAsync(CancellationToken cancellationToken = default)
    {
        if (_status is RealtimeConnectionStatus.Connected or RealtimeConnectionStatus.Connecting)
        {
            return;
        }

        await _startLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_status is RealtimeConnectionStatus.Connected or RealtimeConnectionStatus.Connecting)
            {
                return;
            }

            SetStatus(RealtimeConnectionStatus.Connecting);

            try
            {
                await _connection.StartAsync(cancellationToken).ConfigureAwait(false);
                SetStatus(RealtimeConnectionStatus.Connected);
                await RejoinRequestedGroupsAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Realtime hub connection established.");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Never throw — callers (Production Dashboard / SLA Monitor pages) must
                // continue functioning via their existing ApiClient/manual-refresh path.
                SetStatus(RealtimeConnectionStatus.Faulted);
                _logger.LogWarning(ex, "Realtime hub connection failed to start. Falling back to manual/periodic refresh.");
            }
        }
        finally
        {
            _startLock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task JoinProductionTrackingGroupAsync(CancellationToken cancellationToken = default)
    {
        _productionTrackingGroupRequested = true;

        if (_status != RealtimeConnectionStatus.Connected)
        {
            return;
        }

        await InvokeSafelyAsync(RealtimeHubMethods.JoinProductionTrackingGroup, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task JoinIncidentSlaGroupAsync(CancellationToken cancellationToken = default)
    {
        _incidentSlaGroupRequested = true;

        if (_status != RealtimeConnectionStatus.Connected)
        {
            return;
        }

        await InvokeSafelyAsync(RealtimeHubMethods.JoinIncidentSlaGroup, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public IDisposable OnProductionRunChanged(Action handler)
        => _connection.On(RealtimeClientMethods.ProductionRunChanged, WrapHandler(handler, RealtimeClientMethods.ProductionRunChanged));

    /// <inheritdoc/>
    public IDisposable OnIncidentSlaChanged(Action handler)
        => _connection.On(RealtimeClientMethods.IncidentSlaChanged, WrapHandler(handler, RealtimeClientMethods.IncidentSlaChanged));

    private Action WrapHandler(Action handler, string methodName) => () =>
    {
        try
        {
            handler();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Realtime handler for {MethodName} threw; broadcast dropped.", methodName);
        }
    };

    private async Task RejoinRequestedGroupsAsync(CancellationToken cancellationToken)
    {
        if (_productionTrackingGroupRequested)
        {
            await InvokeSafelyAsync(RealtimeHubMethods.JoinProductionTrackingGroup, cancellationToken).ConfigureAwait(false);
        }

        if (_incidentSlaGroupRequested)
        {
            await InvokeSafelyAsync(RealtimeHubMethods.JoinIncidentSlaGroup, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task InvokeSafelyAsync(string methodName, CancellationToken cancellationToken)
    {
        try
        {
            await _connection.InvokeAsync(methodName, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Realtime hub method {MethodName} failed.", methodName);
        }
    }

    private Task OnReconnectingAsync(Exception? exception)
    {
        SetStatus(RealtimeConnectionStatus.Reconnecting);
        _logger.LogWarning(exception, "Realtime hub connection lost; attempting automatic reconnection.");
        return Task.CompletedTask;
    }

    private async Task OnReconnectedAsync(string? connectionId)
    {
        SetStatus(RealtimeConnectionStatus.Connected);
        _logger.LogInformation("Realtime hub connection restored. ConnectionId={ConnectionId}", connectionId);
        await RejoinRequestedGroupsAsync(CancellationToken.None).ConfigureAwait(false);
    }

    private Task OnClosedAsync(Exception? exception)
    {
        SetStatus(RealtimeConnectionStatus.Faulted);
        _logger.LogWarning(exception, "Realtime hub connection closed permanently. Pages fall back to manual/periodic refresh.");
        return Task.CompletedTask;
    }

    private void SetStatus(RealtimeConnectionStatus status)
    {
        if (_status == status)
        {
            return;
        }

        _status = status;
        StatusChanged?.Invoke(status);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        _connection.Reconnecting -= OnReconnectingAsync;
        _connection.Reconnected -= OnReconnectedAsync;
        _connection.Closed -= OnClosedAsync;

        await _connection.DisposeAsync().ConfigureAwait(false);
        _startLock.Dispose();
    }
}
