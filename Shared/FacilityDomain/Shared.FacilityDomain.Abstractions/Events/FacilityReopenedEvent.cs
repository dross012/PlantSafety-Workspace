// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Events
//  File:           FacilityReopenedEvent.cs
//  Created:        2026-08-08
//  Description:    Domain event raised by FacilityAggregate.ReopenFacility.
//                  Phase C.12 (Path C: Operational Modernization).
// ============================================================================
using Shared.FacilityDomain.Abstractions.ValueObjects;

namespace Shared.FacilityDomain.Abstractions.Events;

/// <summary>Raised when a shut-down facility is reopened.</summary>
public sealed record FacilityReopenedEvent(
    FacilityId FacilityId,
    DateTime OccurredAtUtc);
