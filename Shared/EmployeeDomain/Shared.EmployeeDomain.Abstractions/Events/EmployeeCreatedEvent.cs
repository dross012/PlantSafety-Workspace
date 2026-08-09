// ============================================================================
//  Project:        Shared.EmployeeDomain
//  Module:         Abstractions.Events
//  File:           EmployeeCreatedEvent.cs
//  Created:        2026-08-08
//  Description:    Domain event raised by EmployeeAggregate.CreateEmployee.
//                  Internal to Shared.EmployeeDomain -- publishing this
//                  through Shared.EventBus as an integration event is a
//                  future phase's job, not this one's (mirrors Shared.
//                  FacilityDomain's FacilityCreatedEvent precedent).
// ============================================================================
using Shared.EmployeeDomain.Abstractions.Enums;
using Shared.EmployeeDomain.Abstractions.ValueObjects;

namespace Shared.EmployeeDomain.Abstractions.Events;

/// <summary>Raised when a new employee is created.</summary>
public sealed record EmployeeCreatedEvent(
    EmployeeId EmployeeId,
    string Name,
    EmployeeRole Role,
    DateTime OccurredAtUtc);
