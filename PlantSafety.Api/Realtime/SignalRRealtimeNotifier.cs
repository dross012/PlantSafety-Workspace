// ============================================================================
// File: SignalRRealtimeNotifier.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform
// Created: 2026-07-15
// Description: Presentation-layer implementation of PlantSafety.Application.Abstractions
//   .Realtime.IRealtimeNotifier backed by PlantSafetyRealtimeHub. Registered as a
//   singleton in DI (Program.cs) so any Application-layer command handler can push a
//   "changed" signal to subscribed clients without the Application layer referencing
//   SignalR directly (Clean Architecture — Architecture Specification §4.1).
// SchemaVersion: 1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================================

using Microsoft.AspNetCore.SignalR;
using PlantSafety.Application.Abstractions.Realtime;

namespace PlantSafety.Api.Realtime;

/// <summary>
/// Broadcasts minimal "changed" signals to <see cref="PlantSafetyRealtimeHub"/> group
/// members via <see cref="IHubContext{THub}"/>.
/// </summary>
public sealed class SignalRRealtimeNotifier : IRealtimeNotifier
{
    /// <summary>Client method name invoked for Production Tracking Dashboard changes (Volume T1).</summary>
    public const string ProductionRunChangedClientMethod = "ProductionRunChanged";

    /// <summary>Client method name invoked for Incident SLA Monitor changes (Volume ISM1).</summary>
    public const string IncidentSlaChangedClientMethod = "IncidentSlaChanged";

    private readonly IHubContext<PlantSafetyRealtimeHub> _hubContext;
    private readonly ILogger<SignalRRealtimeNotifier> _logger;

    /// <summary>Initialises a new instance of <see cref="SignalRRealtimeNotifier"/>.</summary>
    /// <param name="hubContext">The SignalR hub context for <see cref="PlantSafetyRealtimeHub"/>.</param>
    /// <param name="logger">Structured logger for broadcast diagnostics.</param>
    public SignalRRealtimeNotifier(IHubContext<PlantSafetyRealtimeHub> hubContext, ILogger<SignalRRealtimeNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task NotifyProductionRunChangedAsync(ProductionRunRealtimeNotification notification, CancellationToken cancellationToken = default)
    {
        var group = RealtimeGroups.ProductionTracking(notification.TenantId);

        await _hubContext.Clients.Group(group).SendAsync(
            ProductionRunChangedClientMethod,
            notification,
            cancellationToken);

        _logger.LogInformation(
            "Realtime broadcast: ProductionRunChanged RunId={RunId} ChangeType={ChangeType} Tenant={TenantId} CorrelationId={CorrelationId}",
            notification.RunId, notification.ChangeType, notification.TenantId, notification.CorrelationId);
    }

    /// <inheritdoc/>
    public async Task NotifyIncidentSlaChangedAsync(IncidentSlaRealtimeNotification notification, CancellationToken cancellationToken = default)
    {
        var group = RealtimeGroups.IncidentSla(notification.TenantId);

        await _hubContext.Clients.Group(group).SendAsync(
            IncidentSlaChangedClientMethod,
            notification,
            cancellationToken);

        _logger.LogInformation(
            "Realtime broadcast: IncidentSlaChanged IncidentId={IncidentId} ChangeType={ChangeType} Tenant={TenantId} CorrelationId={CorrelationId}",
            notification.IncidentId, notification.ChangeType, notification.TenantId, notification.CorrelationId);
    }
}
