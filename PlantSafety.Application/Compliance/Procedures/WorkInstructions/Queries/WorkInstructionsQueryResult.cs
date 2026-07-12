// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetWorkInstructionsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Procedures.WorkInstructions.Models;

namespace PlantSafety.Application.Compliance.Procedures.WorkInstructions.Queries;

/// <summary>Result of <see cref="GetWorkInstructionsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching WorkInstructions items.</param>
public sealed record WorkInstructionsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<WorkInstructionsItemDto> Items);
