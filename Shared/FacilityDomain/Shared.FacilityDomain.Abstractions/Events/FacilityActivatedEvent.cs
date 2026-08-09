// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Events
//  File:           FacilityActivatedEvent.cs
//  Created:        2026-08-08
//  Description:    Domain event raised by FacilityAggregate.ActivateFacility.
//                  Phase C.12 (Path C: Operational Modernization).
// ============================================================================
using Shared.FacilityDomain.Abstractions.ValueObjects;

namespace Shared.FacilityDomain.Abstractions.Events;

/// <summary>Raised when a pending facility is activated for operation.</summary>
public sealed record FacilityActivatedEvent(
    FacilityId FacilityId,
    DateTime OccurredAtUtc);
