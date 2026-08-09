// ============================================================================
//  Project:        Shared.MachineDomain
//  Module:         Abstractions.Events
//  File:           MachineCreatedEvent.cs
//  Created:        2026-08-08
//  Description:    Domain event raised by MachineAggregate.CreateMachine.
//                  Internal to Shared.MachineDomain -- publishing this
//                  through Shared.EventBus as an integration event is a
//                  future phase's job, not this one's.
// ============================================================================
using Shared.MachineDomain.Abstractions.Enums;
using Shared.MachineDomain.Abstractions.Location;
using Shared.MachineDomain.Abstractions.ValueObjects;

namespace Shared.MachineDomain.Abstractions.Events;

/// <summary>Raised when a new machine is created.</summary>
public sealed record MachineCreatedEvent(
    MachineId MachineId,
    string Name,
    MachineType MachineType,
    MachineLocation Location,
    DateTime OccurredAtUtc);
