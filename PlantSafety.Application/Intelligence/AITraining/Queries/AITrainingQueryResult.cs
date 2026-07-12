// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Result payload for GetAITrainingQuery.
// SchemaVersion: 1
using PlantSafety.Application.Intelligence.AITraining.Models;

namespace PlantSafety.Application.Intelligence.AITraining.Queries;

/// <summary>Result of <see cref="GetAITrainingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching AITraining items.</param>
public sealed record AITrainingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<AITrainingItemDto> Items);