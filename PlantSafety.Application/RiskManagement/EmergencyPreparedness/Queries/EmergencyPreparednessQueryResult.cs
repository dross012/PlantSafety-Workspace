// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetEmergencyPreparednessQuery.
// SchemaVersion: 1

using PlantSafety.Application.RiskManagement.EmergencyPreparedness.Models;

namespace PlantSafety.Application.RiskManagement.EmergencyPreparedness.Queries;

/// <summary>Result of <see cref="GetEmergencyPreparednessQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Emergency Preparedness items.</param>
public sealed record EmergencyPreparednessQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EmergencyPreparednessItemDto> Items);