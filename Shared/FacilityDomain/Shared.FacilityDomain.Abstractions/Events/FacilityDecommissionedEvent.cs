// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Events
//  File:           FacilityDecommissionedEvent.cs
//  Created:        2026-08-08
//  Description:    Domain event raised by FacilityAggregate.DecommissionFacility.
//                  Phase C.12 (Path C: Operational Modernization).
// ============================================================================
using Shared.FacilityDomain.Abstractions.ValueObjects;

namespace Shared.FacilityDomain.Abstractions.Events;

/// <summary>Raised when a facility is permanently decommissioned.</summary>
public sealed record FacilityDecommissionedEvent(
    FacilityId FacilityId,
    string Reason,
    DateTime OccurredAtUtc);
