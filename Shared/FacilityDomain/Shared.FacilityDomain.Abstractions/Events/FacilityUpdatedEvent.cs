// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Events
//  File:           FacilityUpdatedEvent.cs
//  Created:        2026-08-08
//  Description:    Domain event raised by every FacilityAggregate Update*
//                  method. One event shape, discriminated by
//                  FacilityUpdateKind, rather than four near-identical
//                  event types -- the update methods differ only in which
//                  piece of state changed, which UpdateKind already
//                  captures.
// ============================================================================
using Shared.FacilityDomain.Abstractions.ValueObjects;

namespace Shared.FacilityDomain.Abstractions.Events;

/// <summary>Which part of a facility's state an <see cref="FacilityUpdatedEvent"/> reflects.</summary>
public enum FacilityUpdateKind
{
    /// <summary>Name/Industry/Location changed (<c>UpdateFacilityMetadata</c>).</summary>
    Metadata,

    /// <summary>The hazard profile changed (<c>UpdateHazardProfile</c>).</summary>
    HazardProfile,

    /// <summary>The regulatory profile changed (<c>UpdateRegulatoryProfile</c>).</summary>
    RegulatoryProfile,

    /// <summary>The program profile changed (<c>UpdateProgramProfile</c>).</summary>
    ProgramProfile,
}

/// <summary>Raised when an existing facility's state changes.</summary>
public sealed record FacilityUpdatedEvent(FacilityId FacilityId, FacilityUpdateKind UpdateKind, DateTime OccurredAtUtc);
