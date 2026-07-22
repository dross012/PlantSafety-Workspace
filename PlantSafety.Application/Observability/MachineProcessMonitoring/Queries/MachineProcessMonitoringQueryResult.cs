// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetMachineProcessMonitoringQuery.
// SchemaVersion: 1

using PlantSafety.Application.Observability.MachineProcessMonitoring.Models;

namespace PlantSafety.Application.Observability.MachineProcessMonitoring.Queries;

/// <summary>Result of <see cref="GetMachineProcessMonitoringQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching MachineProcessMonitoring items.</param>
public sealed record MachineProcessMonitoringQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<MachineProcessMonitoringItemDto> Items);