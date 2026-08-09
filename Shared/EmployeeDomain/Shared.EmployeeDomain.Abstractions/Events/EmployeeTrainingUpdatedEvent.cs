// ============================================================================
//  Project:        Shared.EmployeeDomain
//  Module:         Abstractions.Events
//  File:           EmployeeTrainingUpdatedEvent.cs
//  Created:        2026-08-08
//  Description:    Domain event raised by EmployeeAggregate.UpdateTrainingProfile.
// ============================================================================
using Shared.EmployeeDomain.Abstractions.ValueObjects;

namespace Shared.EmployeeDomain.Abstractions.Events;

/// <summary>Raised when an employee's training profile is replaced.</summary>
public sealed record EmployeeTrainingUpdatedEvent(
    EmployeeId EmployeeId,
    int CompletedTrainingCount,
    DateTime OccurredAtUtc);
