// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Infrastructure.Industries
//  Layer:          Infrastructure
//  File:           EventingServices.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Stateless IEventRoutingService/IEventImpactService/
//                  IEventWorkflowService implementations delegating to the
//                  static EventCatalog/EventRoutingEngine/
//                  EventImpactProfileBuilder/EventWorkflowTemplateCatalog
//                  (Abstractions.Models.Industries.Eventing). Mirrors
//                  IndustryRegistryProvider/RegulatoryRequirementRegistryProvider/
//                  ProgramRegistryProvider/FacilityProfileService exactly.
//                  Stateless, so a single instance of each is safe to share
//                  as a singleton.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.Eventing;
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;

namespace Shared.SafetyData.Infrastructure.Industries;

/// <summary>Stateless <see cref="IEventRoutingService"/> implementation backed by <see cref="EventRoutingEngine"/>.</summary>
public sealed class EventRoutingService : IEventRoutingService
{
    /// <inheritdoc />
    public EventRoutingResult Route(string eventCode, FacilityProfile facility)
    {
        EventDefinition eventDefinition = EventCatalog.FindByCode(eventCode)
            ?? throw new ArgumentException($"No event type named '{eventCode}' exists in EventCatalog.All.", nameof(eventCode));

        return EventRoutingEngine.Route(eventDefinition, facility);
    }
}

/// <summary>Stateless <see cref="IEventImpactService"/> implementation backed by <see cref="EventImpactProfileBuilder"/>.</summary>
public sealed class EventImpactService : IEventImpactService
{
    /// <inheritdoc />
    public EventImpactProfile ComputeImpact(string eventCode, FacilityProfile facility)
    {
        EventDefinition eventDefinition = EventCatalog.FindByCode(eventCode)
            ?? throw new ArgumentException($"No event type named '{eventCode}' exists in EventCatalog.All.", nameof(eventCode));

        return EventImpactProfileBuilder.Build(eventDefinition, facility);
    }
}

/// <summary>Stateless <see cref="IEventWorkflowService"/> implementation backed by <see cref="EventWorkflowTemplateCatalog"/>.</summary>
public sealed class EventWorkflowService : IEventWorkflowService
{
    /// <inheritdoc />
    public EventWorkflowTemplate? GetWorkflow(string eventCode) =>
        EventWorkflowTemplateCatalog.ByEventCode.TryGetValue(eventCode, out EventWorkflowTemplate? template)
            ? template
            : null;
}
