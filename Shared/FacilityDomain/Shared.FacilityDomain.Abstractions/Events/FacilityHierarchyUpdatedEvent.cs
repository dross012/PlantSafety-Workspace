// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Events
//  File:           FacilityHierarchyUpdatedEvent.cs
//  Created:        2026-08-08
//  Description:    Domain event raised by FacilityAggregate's hierarchy
//                  commands (AddBuilding/AddArea/AddZone/UpdateHierarchy).
//                  Phase C.5 (Path C: Operational Modernization).
// ============================================================================
using Shared.FacilityDomain.Abstractions.ValueObjects;

namespace Shared.FacilityDomain.Abstractions.Events;

/// <summary>The kind of change that produced a <see cref="FacilityHierarchyUpdatedEvent"/>.</summary>
public enum FacilityHierarchyChangeKind
{
    /// <summary>A building was added.</summary>
    BuildingAdded,

    /// <summary>An area was added to a building.</summary>
    AreaAdded,

    /// <summary>A zone was added to an area.</summary>
    ZoneAdded,

    /// <summary>The entire hierarchy was replaced.</summary>
    Replaced,
}

/// <summary>Raised when a facility's Building/Area/Zone hierarchy changes.</summary>
public sealed record FacilityHierarchyUpdatedEvent(
    FacilityId FacilityId,
    FacilityHierarchyChangeKind Kind,
    DateTime OccurredAtUtc);
