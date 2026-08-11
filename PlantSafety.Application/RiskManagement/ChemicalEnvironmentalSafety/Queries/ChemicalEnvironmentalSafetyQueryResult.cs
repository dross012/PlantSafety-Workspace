// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetChemicalEnvironmentalSafetyQuery.
// SchemaVersion: 1

using PlantSafety.Application.RiskManagement.ChemicalEnvironmentalSafety.Models;

namespace PlantSafety.Application.RiskManagement.ChemicalEnvironmentalSafety.Queries;

/// <summary>Result of <see cref="GetChemicalEnvironmentalSafetyQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Chemical &amp; Environmental Safety items.</param>
public sealed record ChemicalEnvironmentalSafetyQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ChemicalEnvironmentalSafetyItemDto> Items);