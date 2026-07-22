// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetErgonomicsWorkstationDesignQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyProcedures.ErgonomicsWorkstationDesign.Models;

namespace PlantSafety.Application.SafetyProcedures.ErgonomicsWorkstationDesign.Queries;

/// <summary>Result of <see cref="GetErgonomicsWorkstationDesignQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching ErgonomicsWorkstationDesign items.</param>
public sealed record ErgonomicsWorkstationDesignQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ErgonomicsWorkstationDesignItemDto> Items);