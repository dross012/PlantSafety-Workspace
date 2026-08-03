// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           RouteDefinitionCreatedEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain event raised when a new RouteDefinition aggregate is created. Mirrors
//                  PlantSafety.Domain.ProcessManagement.Events.ProcessDefinitionCreatedEvent.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.ProcessManagement.Routing.Events;

/// <summary>Raised when a new <c>RouteDefinition</c> aggregate is created.</summary>
public sealed record RouteDefinitionCreatedEvent(
    Guid EventId,
    Guid RouteDefinitionId,
    Guid TenantId,
    string Name,
    UtcTimestamp CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRouteDefinitionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RouteDefinitionCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
