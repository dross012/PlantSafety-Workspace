// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Eventing
//  Layer:          Abstractions
//  File:           IEventingServices.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI-friendly abstractions over EventRoutingEngine,
//                  EventImpactProfileBuilder, and EventWorkflowTemplateCatalog,
//                  so a real event domain (AccidentManagement, SafetyEvents,
//                  etc.) can take a constructor dependency instead of a
//                  static reference -- mirrors IIndustryRegistry/
//                  IRegulatoryRequirementRegistry/IProgramRegistry/
//                  IFacilityProfileService's role for their own subsystems
//                  exactly. Three separate interfaces (not one combined
//                  facade), matching the three named services in the
//                  subsystem's brief. All three throw ArgumentException for
//                  an unrecognized event code, consistent with
//                  FacilityProfileBuilder.Build's handling of an unrecognized
//                  industry name -- unlike a browsing lookup (e.g.
//                  IndustryRegistry.FindByName), these methods cannot
//                  produce a meaningful result without a valid event type.
//                  Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;

namespace Shared.SafetyData.Abstractions.Models.Industries.Eventing;

/// <summary>DI-resolvable facade over <see cref="EventRoutingEngine"/>.</summary>
public interface IEventRoutingService
{
    /// <summary>Routes the given event type against the given facility (see <see cref="EventRoutingEngine.Route"/>).</summary>
    /// <exception cref="ArgumentException">No event type named <paramref name="eventCode"/> exists in <see cref="EventCatalog.All"/>.</exception>
    EventRoutingResult Route(string eventCode, FacilityProfile facility);
}

/// <summary>DI-resolvable facade over <see cref="EventImpactProfileBuilder"/>.</summary>
public interface IEventImpactService
{
    /// <summary>Computes the facility-specific impact of the given event type (see <see cref="EventImpactProfileBuilder.Build"/>).</summary>
    /// <exception cref="ArgumentException">No event type named <paramref name="eventCode"/> exists in <see cref="EventCatalog.All"/>.</exception>
    EventImpactProfile ComputeImpact(string eventCode, FacilityProfile facility);
}

/// <summary>DI-resolvable facade over <see cref="EventWorkflowTemplateCatalog"/>.</summary>
public interface IEventWorkflowService
{
    /// <summary>
    /// Returns the response workflow template for the given event code, or <see langword="null"/>
    /// if that event code has no template in <see cref="EventWorkflowTemplateCatalog.ByEventCode"/>
    /// (only the 10 event types in <see cref="EventWorkflowTemplateCatalog.TemplatedEventCodes"/>
    /// have one).
    /// </summary>
    EventWorkflowTemplate? GetWorkflow(string eventCode);
}
