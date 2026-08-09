// ============================================================================
//  Project:        Shared.EmployeeDomain
//  Module:         Abstractions.Events
//  File:           EmployeeCertificationUpdatedEvent.cs
//  Created:        2026-08-08
//  Description:    Domain event raised by EmployeeAggregate.UpdateCertifications.
// ============================================================================
using Shared.EmployeeDomain.Abstractions.ValueObjects;

namespace Shared.EmployeeDomain.Abstractions.Events;

/// <summary>Raised when an employee's certification list is replaced.</summary>
public sealed record EmployeeCertificationUpdatedEvent(
    EmployeeId EmployeeId,
    int CertificationCount,
    DateTime OccurredAtUtc);
