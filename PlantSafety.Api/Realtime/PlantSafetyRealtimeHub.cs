// ============================================================================
// File: PlantSafetyRealtimeHub.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform
// Created: 2026-07-15
// Description: Shared SignalR hub for the two pages whose governing UI wireframe
//   manuals mandate real-time push (not client polling):
//     - Production Tracking Dashboard (UI Production Tracking Architecture, Volume T1
//       — "real-time refresh cycles 1-5s")
//     - Incident SLA Monitor (UI Incident SLA Monitor Wireframe, Volume ISM1 —
//       "SLA/forecast/risk polling is explicitly banned in favor of SignalR hub
//       subscriptions")
//   One hub, typed group-per-module (rather than two separate hubs) to keep the
//   connection/negotiate surface minimal for the MAUI Blazor Hybrid client — a single
//   authenticated WebSocket connection per app session, with server-side group
//   membership determining which broadcasts a client receives.
//   Broadcast payloads are intentionally minimal "changed" signals (see
//   PlantSafety.Application.Abstractions.Realtime.IRealtimeNotifier) — the UI re-fetches
//   full state through its existing typed ApiClient/state-container pipeline. This keeps
//   SLA/throughput computation server-side only (UI001) and avoids a second, divergent
//   projection of the same data.
// SchemaVersion: 1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PlantSafety.Api.Realtime;

/// <summary>
/// Shared, tenant-isolated real-time hub. Authenticated clients join one or more of the
/// canonical <see cref="RealtimeGroups"/> to receive push updates for the Production
/// Tracking Dashboard and/or Incident SLA Monitor.
/// </summary>
[Authorize]
public sealed class PlantSafetyRealtimeHub : Hub
{
    private readonly ILogger<PlantSafetyRealtimeHub> _logger;

    /// <summary>Initialises a new instance of <see cref="PlantSafetyRealtimeHub"/>.</summary>
    /// <param name="logger">Structured logger for connection/group lifecycle events.</param>
    public PlantSafetyRealtimeHub(ILogger<PlantSafetyRealtimeHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Joins the caller's connection to the tenant-scoped Production Tracking Dashboard
    /// group (Volume T1). The caller must carry a valid <c>tenantId</c> claim.
    /// </summary>
    public async Task JoinProductionTrackingGroupAsync()
    {
        var tenantId = ResolveTenantId();
        if (tenantId is null)
        {
            _logger.LogWarning("Realtime hub: JoinProductionTrackingGroupAsync rejected — missing tenantId claim. ConnectionId={ConnectionId}", Context.ConnectionId);
            throw new HubException("Missing tenant context.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroups.ProductionTracking(tenantId.Value));
        _logger.LogInformation("Realtime hub: connection {ConnectionId} joined ProductionTracking group for Tenant {TenantId}", Context.ConnectionId, tenantId);
    }

    /// <summary>
    /// Joins the caller's connection to the tenant-scoped Incident SLA Monitor group
    /// (Volume ISM1). The caller must carry a valid <c>tenantId</c> claim.
    /// </summary>
    public async Task JoinIncidentSlaGroupAsync()
    {
        var tenantId = ResolveTenantId();
        if (tenantId is null)
        {
            _logger.LogWarning("Realtime hub: JoinIncidentSlaGroupAsync rejected — missing tenantId claim. ConnectionId={ConnectionId}", Context.ConnectionId);
            throw new HubException("Missing tenant context.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroups.IncidentSla(tenantId.Value));
        _logger.LogInformation("Realtime hub: connection {ConnectionId} joined IncidentSla group for Tenant {TenantId}", Context.ConnectionId, tenantId);
    }

    /// <inheritdoc/>
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // Group membership is cleaned up automatically by SignalR when a connection drops;
        // no explicit RemoveFromGroupAsync calls are required here.
        if (exception is not null)
        {
            _logger.LogWarning(exception, "Realtime hub: connection {ConnectionId} disconnected with error.", Context.ConnectionId);
        }

        return base.OnDisconnectedAsync(exception);
    }

    private Guid? ResolveTenantId()
    {
        var raw = Context.User?.FindFirstValue("tenantId");
        return Guid.TryParse(raw, out var tenantId) ? tenantId : null;
    }
}
