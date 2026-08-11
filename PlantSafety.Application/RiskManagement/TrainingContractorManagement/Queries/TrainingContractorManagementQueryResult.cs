// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetTrainingContractorManagementQuery.
// SchemaVersion: 1

using PlantSafety.Application.RiskManagement.TrainingContractorManagement.Models;

namespace PlantSafety.Application.RiskManagement.TrainingContractorManagement.Queries;

/// <summary>Result of <see cref="GetTrainingContractorManagementQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Training &amp; Contractor Management items.</param>
public sealed record TrainingContractorManagementQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<TrainingContractorManagementItemDto> Items);