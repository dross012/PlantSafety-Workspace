// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetEnvironmentalSamplingQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.EnvironmentalSampling.Models;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Inspections.EnvironmentalSampling.Queries;

/// <summary>Result of <see cref="GetEnvironmentalSamplingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching EnvironmentalSampling items.</param>
public sealed record EnvironmentalSamplingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EnvironmentalSamplingItemDto> Items);
