// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Application-layer abstraction for pushing lightweight "changed"
//   notifications to connected real-time clients (SignalR). The Application layer
//   never references SignalR/AspNetCore types directly (Clean Architecture — see
//   Architecture Specification Section 4.1); the Presentation layer (PlantSafety.Api)
//   supplies the concrete implementation (SignalRRealtimeNotifier) and registers it
//   in DI. Handlers call this interface exactly as they already call ILogger — a
//   thin, fire-and-forget side effect after the write succeeds.
//   Payloads are intentionally minimal ("cache invalidation" signals, not full
//   projections) so the UI re-fetches through its existing typed ApiClient/state
//   container pipeline rather than duplicating server-side computation (SLA
//   remaining, throughput, etc.) inside the real-time payload itself.
// SchemaVersion: 1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI

namespace PlantSafety.Application.Abstractions.Realtime;

/// <summary>
/// Publishes real-time "changed" notifications for modules that require SignalR-pushed
/// UI updates instead of client-side polling (Production Tracking Dashboard, Incident
/// SLA Monitor — see UI Production Tracking Architecture Volume T1 and Incident SLA
/// Monitor Wireframe Volume ISM1).
/// </summary>
public interface IRealtimeNotifier
{
    /// <summary>
    /// Notifies subscribers of the Production Tracking real-time group that a
    /// production run's status/throughput/bottleneck state has changed.
    /// </summary>
    /// <param name="notification">The minimal change signal.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task NotifyProductionRunChangedAsync(ProductionRunRealtimeNotification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies subscribers of the Incident SLA real-time group that an incident's
    /// SLA-relevant state (severity, status, technician assignment, escalation) has changed.
    /// </summary>
    /// <param name="notification">The minimal change signal.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task NotifyIncidentSlaChangedAsync(IncidentSlaRealtimeNotification notification, CancellationToken cancellationToken = default);
}

/// <summary>
/// Minimal "production run changed" signal. Intentionally excludes computed
/// throughput/bottleneck values — the UI re-fetches those via <c>ProductionTrackingApiClient</c>.
/// </summary>
/// <param name="TenantId">The owning tenant.</param>
/// <param name="RunId">The affected production run identifier.</param>
/// <param name="ChangeType">The nature of the change (Created, StatusChanged, ThroughputChanged, Completed).</param>
/// <param name="CorrelationId">The correlation identifier of the originating command.</param>
/// <param name="TimestampUtc">The UTC timestamp the change occurred.</param>
public sealed record ProductionRunRealtimeNotification(
    Guid TenantId,
    Guid RunId,
    string ChangeType,
    Guid CorrelationId,
    DateTime TimestampUtc);

/// <summary>
/// Minimal "incident SLA state changed" signal. Intentionally excludes computed SLA
/// remaining/risk classification — the UI re-fetches those via <c>IncidentManagementApiClient</c>.
/// </summary>
/// <param name="TenantId">The owning tenant.</param>
/// <param name="IncidentId">The affected incident identifier.</param>
/// <param name="ChangeType">The nature of the change (Created, Updated, Escalated, TechnicianAssigned, Resolved, Breached).</param>
/// <param name="CorrelationId">The correlation identifier of the originating command.</param>
/// <param name="TimestampUtc">The UTC timestamp the change occurred.</param>
public sealed record IncidentSlaRealtimeNotification(
    Guid TenantId,
    Guid IncidentId,
    string ChangeType,
    Guid CorrelationId,
    DateTime TimestampUtc);
