// ============================================================================
//  Project:        Shared.EmployeeDomain.Infrastructure
//  Module:         Events
//  File:           EmployeeIntegrationEvents.cs
//  Created:        2026-08-10
//  Description:    Phase 4 (Employee domain pass), Section 4 "Domain
//                  Events": the integration-event shapes EmployeeService
//                  publishes through Shared.ModuleContracts' IModuleEventGateway
//                  after a successful command. Distinct from
//                  Shared.EmployeeDomain.Abstractions.Events' plain domain
//                  events, mirroring the Shared.FacilityDomain.Infrastructure.
//                  Events precedent established in the Facility domain pass.
// ============================================================================
using Shared.ModuleContracts.Abstractions.Events;

namespace Shared.EmployeeDomain.Infrastructure.Events;

/// <summary>Published after a new employee is created.</summary>
public sealed record EmployeeCreatedEvent(string SourceModule, DateTime OccurredAtUtc, Guid EmployeeId, string Name) : IModuleEvent;

/// <summary>Published after an existing employee's metadata/injury profile/exposure records/safety score changes.</summary>
public sealed record EmployeeUpdatedEvent(string SourceModule, DateTime OccurredAtUtc, Guid EmployeeId, string UpdateKind) : IModuleEvent;

/// <summary>Published after an employee's completed training codes are replaced.</summary>
public sealed record EmployeeTrainingUpdatedEvent(string SourceModule, DateTime OccurredAtUtc, Guid EmployeeId, int CompletedTrainingCount) : IModuleEvent;

/// <summary>Published after an employee's certifications are replaced.</summary>
public sealed record EmployeeCertificationUpdatedEvent(string SourceModule, DateTime OccurredAtUtc, Guid EmployeeId, int CertificationCount) : IModuleEvent;
