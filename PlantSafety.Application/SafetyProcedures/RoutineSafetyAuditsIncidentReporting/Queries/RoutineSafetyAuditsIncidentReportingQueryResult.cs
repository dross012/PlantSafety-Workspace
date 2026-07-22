// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetRoutineSafetyAuditsIncidentReportingQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Models;

namespace PlantSafety.Application.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Queries;

/// <summary>Result of <see cref="GetRoutineSafetyAuditsIncidentReportingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching RoutineSafetyAuditsIncidentReporting items.</param>
public sealed record RoutineSafetyAuditsIncidentReportingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<RoutineSafetyAuditsIncidentReportingItemDto> Items);