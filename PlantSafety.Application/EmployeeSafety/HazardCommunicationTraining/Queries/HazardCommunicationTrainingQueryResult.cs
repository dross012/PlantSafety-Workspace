// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetHazardCommunicationTrainingQuery.
// SchemaVersion: 1

using PlantSafety.Application.EmployeeSafety.HazardCommunicationTraining.Models;

namespace PlantSafety.Application.EmployeeSafety.HazardCommunicationTraining.Queries;

/// <summary>Result of <see cref="GetHazardCommunicationTrainingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching HazardCommunicationTraining items.</param>
public sealed record HazardCommunicationTrainingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<HazardCommunicationTrainingItemDto> Items);