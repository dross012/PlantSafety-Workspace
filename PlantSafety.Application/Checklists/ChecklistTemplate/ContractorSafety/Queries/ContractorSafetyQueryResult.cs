// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetContractorSafetyQuery.
// SchemaVersion: 1

using PlantSafety.Application.Checklists.ChecklistTemplate.ContractorSafety.Models;

namespace PlantSafety.Application.Checklists.ChecklistTemplate.ContractorSafety.Queries;

/// <summary>Result of <see cref="GetContractorSafetyQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching ContractorSafety items.</param>
public sealed record ContractorSafetyQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ContractorSafetyItemDto> Items);