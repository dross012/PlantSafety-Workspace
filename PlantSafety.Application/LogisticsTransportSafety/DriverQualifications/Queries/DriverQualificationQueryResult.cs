// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for GetDriverQualificationStatusQuery.
// SchemaVersion: 1

using PlantSafety.Application.LogisticsTransportSafety.DriverQualifications.Models;

namespace PlantSafety.Application.LogisticsTransportSafety.DriverQualifications.Queries;

/// <summary>Result of <see cref="GetDriverQualificationStatusQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="DriverUserId">Driver the data belongs to.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching DriverQualification items for this driver.</param>
public sealed record DriverQualificationQueryResult(
    Guid TenantId,
    Guid DriverUserId,
    DateTime GeneratedAt,
    IReadOnlyList<DriverQualificationItemDto> Items);
