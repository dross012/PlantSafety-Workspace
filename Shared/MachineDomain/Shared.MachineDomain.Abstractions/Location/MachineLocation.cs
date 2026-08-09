// ============================================================================
//  Project:        Shared.MachineDomain
//  Module:         Abstractions.Location
//  File:           MachineLocation.cs
//  Created:        2026-08-08
//  Description:    Phase C.2: where a machine physically sits, anchored to
//                  a real Shared.FacilityDomain FacilityId (facility-aware
//                  by construction) plus a free-text area description.
//                  Phase C.5 (Facility Hierarchy) may later let this
//                  reference a BuildingId/AreaId/ZoneId directly -- kept as
//                  a plain string here since that hierarchy does not exist
//                  yet at this point in the build.
// ============================================================================
using Shared.FacilityDomain.Abstractions.ValueObjects;

namespace Shared.MachineDomain.Abstractions.Location;

/// <summary>Where a machine physically sits within a facility.</summary>
public sealed record MachineLocation(
    FacilityId FacilityId,
    string AreaDescription);
