// ============================================================================
//  Project:        Shared.MachineDomain.Infrastructure
//  Module:         Events
//  File:           MachineIntegrationEvents.cs
//  Created:        2026-08-10
//  Description:    Phase 4 (Machine domain pass), Section 4 "Domain
//                  Events": the integration-event shapes MachineService
//                  publishes through Shared.ModuleContracts' IModuleEventGateway
//                  after a successful command. Distinct from
//                  Shared.MachineDomain.Abstractions.Events' plain domain
//                  events, mirroring the Shared.FacilityDomain.Infrastructure.
//                  Events / Shared.EmployeeDomain.Infrastructure.Events
//                  precedent established in the Facility and Employee
//                  domain passes.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.ModuleContracts.Abstractions.Events;

namespace Shared.MachineDomain.Infrastructure.Events;

/// <summary>Published after a new machine is created.</summary>
public sealed record MachineCreatedEvent(string SourceModule, DateTime OccurredAtUtc, Guid MachineId, string Name) : IModuleEvent;

/// <summary>Published after an existing machine's metadata or safety score changes.</summary>
public sealed record MachineUpdatedEvent(string SourceModule, DateTime OccurredAtUtc, Guid MachineId, string UpdateKind) : IModuleEvent;

/// <summary>Published after a machine's maintenance profile is replaced.</summary>
public sealed record MachineMaintenanceUpdatedEvent(string SourceModule, DateTime OccurredAtUtc, Guid MachineId, DateTime? NextScheduledMaintenanceAtUtc) : IModuleEvent;

/// <summary>Published after a failure is recorded against a machine. Carries the resulting HazardType so a downstream hazard-propagation subscriber can react without re-deriving it.</summary>
public sealed record MachineFailureRecordedEvent(string SourceModule, DateTime OccurredAtUtc, Guid MachineId, string Description, HazardType ResultingHazardType) : IModuleEvent;
