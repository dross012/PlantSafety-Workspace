// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Events
//  File:           FacilityShutdownEvent.cs
//  Created:        2026-08-08
//  Description:    Domain event raised by FacilityAggregate.ShutdownFacility.
//                  Phase C.12 (Path C: Operational Modernization).
// ============================================================================
using Shared.FacilityDomain.Abstractions.ValueObjects;

namespace Shared.FacilityDomain.Abstractions.Events;

/// <summary>Raised when an active facility is temporarily shut down.</summary>
public sealed record FacilityShutdownEvent(
    FacilityId FacilityId,
    string Reason,
    DateTime OccurredAtUtc);
