// ============================================================================
//  Project:        Shared.FacilityDomain.Infrastructure
//  Module:         Events
//  File:           FacilityIntegrationEvents.cs
//  Created:        2026-08-10
//  Description:    Phase 4 (Facility domain pass), Section 4 "Domain
//                  Events": the integration-event shapes FacilityService
//                  publishes through Shared.ModuleContracts' IModuleEventGateway
//                  after a successful command. Distinct from
//                  Shared.FacilityDomain.Abstractions.Events' plain domain
//                  events (FacilityCreatedEvent, FacilityUpdatedEvent, etc.)
//                  -- those stay pure domain facts with no IModuleEvent
//                  dependency; these are the thin, IModuleEvent-implementing
//                  wrappers translated from them at the Infrastructure
//                  boundary, mirroring the pattern Phase 2.7 established in
//                  PlantSafety.UI.Api (a controller-level FacilityCreatedEvent
//                  distinct from the domain's own). Relocating that
//                  publish responsibility here (Section 2.3: "Handlers must
//                  emit domain events") is what let FacilityCreatedEvent's
//                  publish move out of FacilitiesController, which had been
//                  the only Facility event ever actually published --
//                  FacilityUpdatedEvent/FacilityHierarchyUpdatedEvent/
//                  FacilityActivatedEvent/FacilityShutdownEvent/
//                  FacilityReopenedEvent/FacilityDecommissionedEvent were
//                  collected by FacilityAggregate and then silently
//                  discarded by FacilityService.ApplyUpdateAsync's
//                  ClearDomainEvents() call, with nothing ever consuming
//                  them.
// ============================================================================
using Shared.ModuleContracts.Abstractions.Events;

namespace Shared.FacilityDomain.Infrastructure.Events;

/// <summary>Published after a new facility is created.</summary>
public sealed record FacilityCreatedEvent(string SourceModule, DateTime OccurredAtUtc, Guid FacilityId, string Name) : IModuleEvent;

/// <summary>Published after an existing facility's metadata/hazard/regulatory/program profile changes.</summary>
public sealed record FacilityUpdatedEvent(string SourceModule, DateTime OccurredAtUtc, Guid FacilityId, string UpdateKind) : IModuleEvent;

/// <summary>Published after a facility's Building/Area/Zone hierarchy changes.</summary>
public sealed record FacilityHierarchyUpdatedEvent(string SourceModule, DateTime OccurredAtUtc, Guid FacilityId, string ChangeKind) : IModuleEvent;

/// <summary>Published after a facility's lifecycle status changes (activated/shut down/reopened/decommissioned).</summary>
public sealed record FacilityLifecycleChangedEvent(string SourceModule, DateTime OccurredAtUtc, Guid FacilityId, string NewStatus, string? Reason) : IModuleEvent;
