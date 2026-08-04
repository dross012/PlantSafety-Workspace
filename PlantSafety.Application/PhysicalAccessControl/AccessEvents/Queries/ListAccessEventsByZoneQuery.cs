// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to list AccessEvents for a given SafetyZone within a date range.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PhysicalAccessControl.AccessEvents.Queries;

/// <summary>
/// Lists AccessEvents for the given tenant/zone within the inclusive UTC date range
/// [<paramref name="From"/>, <paramref name="To"/>].
/// </summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SafetyZoneId">SafetyZone to filter events by.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
public sealed record ListAccessEventsByZoneQuery(
    Guid TenantId,
    Guid SafetyZoneId,
    DateTime From,
    DateTime To) : IRequest<Result<ListAccessEventsByZoneResult>>;
