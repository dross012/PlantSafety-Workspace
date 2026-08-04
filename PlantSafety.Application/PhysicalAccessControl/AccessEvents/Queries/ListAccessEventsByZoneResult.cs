// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for ListAccessEventsByZoneQuery.
// SchemaVersion: 1

using PlantSafety.Application.PhysicalAccessControl.AccessEvents.Models;

namespace PlantSafety.Application.PhysicalAccessControl.AccessEvents.Queries;

/// <summary>Result of <see cref="ListAccessEventsByZoneQuery"/>.</summary>
/// <param name="SafetyZoneId">SafetyZone the events belong to.</param>
/// <param name="Items">Matching AccessEvent items.</param>
public sealed record ListAccessEventsByZoneResult(
    Guid SafetyZoneId,
    IReadOnlyList<AccessEventDto> Items);
