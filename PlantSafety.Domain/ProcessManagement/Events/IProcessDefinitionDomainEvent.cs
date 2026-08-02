// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Domain
//  File:           IProcessDefinitionDomainEvent.cs
//  Created:        2026-07-15
//  Updated:        2026-08-02 -- Phase 4 full build-out: now extends the platform-canonical
//                  PlantSafety.Domain.Common.Abstractions.IDomainEvent (previously declared its
//                  own standalone copy of the same four members) so ProcessDefinition domain
//                  events can flow through the shared IOutboxWriter.Enqueue(IDomainEvent, ...)
//                  pipeline used by every other module. Purely additive -- the member shapes are
//                  identical, so no existing implementer needs any change.
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all ProcessDefinition domain events.
//                  Carries the canonical event envelope required by §2.13.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ProcessManagement.Events;

/// <summary>Marker interface for domain events raised by the <c>ProcessDefinition</c> aggregate.</summary>
public interface IProcessDefinitionDomainEvent : IDomainEvent
{
}
