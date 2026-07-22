// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetProcessSafetyInformationQuery.
// SchemaVersion: 1

using PlantSafety.Application.RiskManagement.ProcessSafetyInformation.Models;

namespace PlantSafety.Application.RiskManagement.ProcessSafetyInformation.Queries;

/// <summary>Result of <see cref="GetProcessSafetyInformationQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Process Safety Information (PSI) items.</param>
public sealed record ProcessSafetyInformationQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ProcessSafetyInformationItemDto> Items);