// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for GetZoneOccupancyFromAccessEventsQuery.
// SchemaVersion: 1

using PlantSafety.Application.PhysicalAccessControl.SafetyZones.Models;

namespace PlantSafety.Application.PhysicalAccessControl.SafetyZones.Queries;

/// <summary>Result of <see cref="GetZoneOccupancyFromAccessEventsQuery"/>.</summary>
/// <param name="Occupancy">The matching SafetyZone occupancy projection, or null if not found.</param>
public sealed record GetZoneOccupancyFromAccessEventsResult(SafetyZoneOccupancyDto? Occupancy);
