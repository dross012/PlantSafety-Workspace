// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetEmergencyProcedureQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Procedures.EmergencyProcedure.Models;

namespace PlantSafety.Application.Compliance.Procedures.EmergencyProcedure.Queries;

/// <summary>Result of <see cref="GetEmergencyProcedureQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching EmergencyProcedure items.</param>
public sealed record EmergencyProcedureQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EmergencyProcedureItemDto> Items);
