// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetHazardIdentificationMeasurementQuery.
// SchemaVersion: 1

using PlantSafety.Application.RiskManagement.HazardIdentificationMeasurement.Models;

namespace PlantSafety.Application.RiskManagement.HazardIdentificationMeasurement.Queries;

/// <summary>Result of <see cref="GetHazardIdentificationMeasurementQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Hazard Identification &amp; Measurement items.</param>
public sealed record HazardIdentificationMeasurementQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<HazardIdentificationMeasurementItemDto> Items);