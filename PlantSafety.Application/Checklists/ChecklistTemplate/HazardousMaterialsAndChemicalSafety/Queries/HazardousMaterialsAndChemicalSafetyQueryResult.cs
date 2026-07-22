// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetHazardousMaterialsAndChemicalSafetyQuery.
// SchemaVersion: 1

using PlantSafety.Application.Checklists.ChecklistTemplate.HazardousMaterialsAndChemicalSafety.Models;

namespace PlantSafety.Application.Checklists.ChecklistTemplate.HazardousMaterialsAndChemicalSafety.Queries;

/// <summary>Result of <see cref="GetHazardousMaterialsAndChemicalSafetyQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching HazardousMaterialsAndChemicalSafety items.</param>
public sealed record HazardousMaterialsAndChemicalSafetyQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<HazardousMaterialsAndChemicalSafetyItemDto> Items);