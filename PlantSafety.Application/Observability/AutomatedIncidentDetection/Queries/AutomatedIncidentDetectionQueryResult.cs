// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetAutomatedIncidentDetectionQuery.
// SchemaVersion: 1

using PlantSafety.Application.Observability.AutomatedIncidentDetection.Models;

namespace PlantSafety.Application.Observability.AutomatedIncidentDetection.Queries;

/// <summary>Result of <see cref="GetAutomatedIncidentDetectionQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching AutomatedIncidentDetection items.</param>
public sealed record AutomatedIncidentDetectionQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<AutomatedIncidentDetectionItemDto> Items);