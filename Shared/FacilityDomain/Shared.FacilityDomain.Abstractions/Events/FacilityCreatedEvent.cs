// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Events
//  File:           FacilityCreatedEvent.cs
//  Created:        2026-08-08
//  Description:    Domain event raised by FacilityAggregate.CreateFacility.
//                  Internal to Shared.FacilityDomain -- publishing this
//                  through Shared.EventBus (as an integration event) is
//                  Phase B.4's job, not this one's.
// ============================================================================
using Shared.FacilityDomain.Abstractions.ValueObjects;

namespace Shared.FacilityDomain.Abstractions.Events;

/// <summary>Raised when a new facility is created.</summary>
public sealed record FacilityCreatedEvent(
    FacilityId FacilityId,
    string Name,
    FacilityIndustry Industry,
    FacilityLocation Location,
    DateTime OccurredAtUtc);
