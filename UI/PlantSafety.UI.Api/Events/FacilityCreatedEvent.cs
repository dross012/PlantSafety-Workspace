// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Events
//  File:           FacilityCreatedEvent.cs
//  Created:        2026-08-09
//  Description:    Phase 2.7: the first real module event published
//                  through Shared.ModuleContracts' IModuleEventGateway.
//                  Published by FacilitiesController after a facility is
//                  created; subscribed to at startup (Program.cs) by a
//                  handler that logs a structured event via
//                  ILogEventWriter -- proving the gateway carries a real
//                  fact from a publisher to a subscriber that has no
//                  direct reference to the publisher, rather than staying
//                  unused plumbing.
// ============================================================================
using Shared.ModuleContracts.Abstractions.Events;

namespace PlantSafety.UI.Api.Events;

/// <summary>Published after a new facility is created.</summary>
public sealed record FacilityCreatedEvent(string SourceModule, DateTime OccurredAtUtc, Guid FacilityId, string Name) : IModuleEvent;
