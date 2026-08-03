// AUTHOR: PlantSafety Platform Team (Claude Code performance-optimization-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Result payload for GetPlatformHealthDashboardQuery.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Dashboard.PlatformHealthDashboard.Models;
using PlantSafety.Application.Messaging.Models;

namespace PlantSafety.Application.Analytics.Dashboard.PlatformHealthDashboard.Queries;

/// <summary>Result of <see cref="GetPlatformHealthDashboardQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Request throughput/latency/error-rate KPI items, one per business hour.</param>
/// <param name="EventBusHealth">Event-bus/DLQ health summary, grouped by EventType, reused directly
/// from the existing Phase 6 dead-letter analytics infrastructure (no parallel system).</param>
public sealed record PlatformHealthDashboardQueryResult(
    Guid TenantId,
    DateTime GeneratedAt,
    IReadOnlyList<PlatformHealthDashboardItemDto> Items,
    IReadOnlyList<DeadLetterEventsSummaryDto> EventBusHealth);
