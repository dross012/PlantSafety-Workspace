// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           IRouteDefinitionDomainEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all RouteDefinition domain events. Extends the
//                  platform-canonical PlantSafety.Domain.Common.Abstractions.IDomainEvent so
//                  RouteDefinition events flow through the shared IOutboxWriter pipeline used by
//                  every other module. Mirrors
//                  PlantSafety.Domain.ProcessManagement.Events.IProcessDefinitionDomainEvent.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Domain.ProcessManagement.Routing.Events;

/// <summary>Marker interface for domain events raised by the <c>RouteDefinition</c> aggregate.</summary>
public interface IRouteDefinitionDomainEvent : IDomainEvent
{
}
