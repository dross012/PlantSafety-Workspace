// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Eventing
//  Layer:          Abstractions
//  File:           EventRoutingEngine.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    The core computation of the Eventing & Orchestration
//                  subsystem: given an event type and a facility, determines
//                  which programs activate, which SOPs activate, which
//                  training is required, which audits are triggered, which
//                  permits are impacted, which regulators are involved, which
//                  dashboard alerts fire, and which compliance elements
//                  change state (become relevant corrective actions).
//
//                  This is a PURE FUNCTION, not a live routing engine in the
//                  message-bus sense: it does not subscribe to
//                  Shared.EventBus, does not consume real domain events, and
//                  does not dispatch to real handlers. "Routing" here means
//                  "determine which of the facility's own reference-derived
//                  requirements this event type activates", not "deliver a
//                  message". See EventDefinition.cs's ARCHITECTURAL SCOPE
//                  note for the full rationale -- a real event domain
//                  (AccidentManagement, SafetyEvents, etc.) calls
//                  IEventRoutingService.Route after it has already received
//                  and persisted a real event; this engine never receives
//                  events itself.
//
//                  Integration with the Facility Profile subsystem: EventBusTopic
//                  reuses the exact same ProgramEventRoutingRules.TopicFor
//                  computation FacilityEventRoutingRulesBuilder already uses
//                  per required program, so a facility's per-program event
//                  routes (FacilityEventRoutingRules.cs) and this engine's
//                  per-event-occurrence routing always agree on topic names
//                  for the same program.
//                  Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.Consumers;
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;

namespace Shared.SafetyData.Abstractions.Models.Industries.Eventing;

/// <summary>
/// The full routing result for a single event type occurring at a specific facility: everything
/// that activates, triggers, or changes state as a result.
/// </summary>
/// <param name="EventCode">The event type this result was computed for.</param>
/// <param name="FacilityId">The facility this result was computed for.</param>
/// <param name="ActivatedPrograms">Safety Programs this event activates (<see cref="EventDefinition.RelatedPrograms"/>).</param>
/// <param name="ActivatedSops">SOPs this event activates (<see cref="EventDefinition.RequiredSops"/>).</param>
/// <param name="RequiredTraining">Training this event requires (<see cref="EventDefinition.RequiredTraining"/>).</param>
/// <param name="TriggeredAudits">Audit checklist items this event triggers (one per <see cref="EventDefinition.RelatedPrograms"/>).</param>
/// <param name="ImpactedPermits">Permit types this event impacts (<see cref="EventDefinition.RequiredPermits"/>).</param>
/// <param name="InvolvedRegulators">Regulators this event involves (<see cref="EventDefinition.RelatedRegulators"/>).</param>
/// <param name="DashboardAlerts">Deterministic alert strings for this occurrence.</param>
/// <param name="ChangedComplianceElements">The facility's own compliance elements this event touches -- the union of activated Program/SOP/Training/Permit elements that also appear in the facility's <see cref="ComplianceElement"/> set, regardless of completion state.</param>
/// <param name="EventBusTopics">The event-bus topic(s) this occurrence's downstream events should route to, one per activated program (<see cref="ProgramEventRoutingRules.TopicFor"/>), matching <see cref="FacilityEventRoutingRulesBuilder"/>'s own per-program topic computation.</param>
public sealed record EventRoutingResult(
    string EventCode,
    Guid FacilityId,
    IReadOnlyList<ProgramDefinition> ActivatedPrograms,
    IReadOnlyList<SopTemplate> ActivatedSops,
    IReadOnlyList<TrainingRequirement> RequiredTraining,
    IReadOnlyList<AuditChecklistItem> TriggeredAudits,
    IReadOnlyList<PermitType> ImpactedPermits,
    IReadOnlyList<Regulator> InvolvedRegulators,
    IReadOnlyList<string> DashboardAlerts,
    IReadOnlyList<ComplianceElement> ChangedComplianceElements,
    IReadOnlyList<string> EventBusTopics);

/// <summary>Computes <see cref="EventRoutingResult"/> deterministically from an <see cref="EventDefinition"/> and a <see cref="FacilityProfile"/>.</summary>
public static class EventRoutingEngine
{
    /// <summary>Routes the given event type against the given facility.</summary>
    public static EventRoutingResult Route(EventDefinition eventDefinition, FacilityProfile facility)
    {
        var activatedElementCodes = eventDefinition.RequiredSops.Select(sop => sop.Code)
            .Concat(eventDefinition.RequiredTraining.Select(training => training.Code))
            .Concat(eventDefinition.RequiredPermits.Select(permit => permit.ToString()))
            .Concat(eventDefinition.RelatedPrograms.Select(program => program.Code))
            .ToHashSet(StringComparer.Ordinal);

        IReadOnlyList<ComplianceElement> changedComplianceElements = facility.ComplianceProfile.RequiredElements
            .Where(element => activatedElementCodes.Contains(element.Code))
            .ToArray();

        IReadOnlyList<string> dashboardAlerts = new[]
        {
            $"{eventDefinition.Title} ({eventDefinition.Severity}) reported at facility.",
        }
        .Concat(changedComplianceElements
            .Where(element => facility.ComplianceProfile.MissingElements.Contains(element))
            .Select(element => $"Related compliance gap: {element.Description}"))
        .ToArray();

        IReadOnlyList<string> eventBusTopics = eventDefinition.RelatedPrograms
            .Select(ProgramEventRoutingRules.TopicFor)
            .Distinct()
            .ToArray();

        return new EventRoutingResult(
            EventCode: eventDefinition.EventCode,
            FacilityId: facility.FacilityId,
            ActivatedPrograms: eventDefinition.RelatedPrograms,
            ActivatedSops: eventDefinition.RequiredSops,
            RequiredTraining: eventDefinition.RequiredTraining,
            TriggeredAudits: eventDefinition.RelatedPrograms.Select(ProgramAuditRequirements.For).ToArray(),
            ImpactedPermits: eventDefinition.RequiredPermits,
            InvolvedRegulators: eventDefinition.RelatedRegulators,
            DashboardAlerts: dashboardAlerts,
            ChangedComplianceElements: changedComplianceElements,
            EventBusTopics: eventBusTopics);
    }
}
