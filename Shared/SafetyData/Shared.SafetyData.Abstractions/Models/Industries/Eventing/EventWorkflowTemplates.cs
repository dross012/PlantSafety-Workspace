// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Eventing
//  Layer:          Abstractions
//  File:           EventWorkflowTemplates.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic 5-step response workflow templates for the
//                  10 event types named in this subsystem's brief (Chemical
//                  spill, Injury, Fire, DOT hazmat incident, Environmental
//                  release, Confined space event, LOTO event, Hot work
//                  event, Fleet incident, Explosive material event). Every
//                  template uses the SAME 5-step shape (Notify, Contain,
//                  Investigate, Remediate, ReportAndClose), populated purely
//                  from the corresponding EventCatalog entry's own
//                  Title/RelatedRegulators/RequiredSops -- never a distinct,
//                  hand-invented workflow per event. A uniform, mechanically-
//                  generated template is the only way to produce 10
//                  "deterministic" workflows without fabricating
//                  event-specific step counts/ordering this subsystem has no
//                  real source data for (PlantSafety's actual
//                  AccidentManagement/SafetyEvents domains, not consulted
//                  here, are the real source of truth for actual response
//                  procedures -- see EventDefinition.cs's ARCHITECTURAL SCOPE
//                  note).
//
//                  Naming substitution: the brief's "Explosive material
//                  event" has no dedicated flagged EventCatalog entry of its
//                  own that carries SOPs/training (AtfExplosiveMaterialDeviation
//                  is a generic, unflagged regulatory-violation event, by
//                  design -- see EventCatalog.cs). The catalog's ExplosionRisk
//                  event (SafetyFlag.ExplosionRisk, Regulator.ATF) is the
//                  event this workflow can actually act on with real SOPs/
//                  training/permits, so it is used in ExplosiveMaterialEvent's
//                  place.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.Eventing;

/// <summary>A single step in an <see cref="EventWorkflowTemplate"/>.</summary>
/// <param name="Order">1-based step order.</param>
/// <param name="Name">Short step name.</param>
/// <param name="Description">What the step requires, populated from the source <see cref="EventDefinition"/>.</param>
public sealed record WorkflowStep(int Order, string Name, string Description);

/// <summary>The deterministic response workflow for a single event type.</summary>
/// <param name="EventCode">The event type this workflow was built for.</param>
/// <param name="Steps">The workflow's steps, in order.</param>
public sealed record EventWorkflowTemplate(string EventCode, IReadOnlyList<WorkflowStep> Steps);

/// <summary>
/// Deterministic, uniform-shape response workflow templates for the 10 event types named in the
/// Eventing &amp; Orchestration subsystem's brief.
/// </summary>
public static class EventWorkflowTemplateCatalog
{
    /// <summary>
    /// The event codes this catalog builds workflows for, in the order named in the subsystem
    /// brief. "ExplosionRisk" stands in for "Explosive material event" -- see this file's header.
    /// </summary>
    public static IReadOnlyList<string> TemplatedEventCodes { get; } = new[]
    {
        "ChemicalSpill",
        "Injury",
        "Fire",
        "DotHazmatIncident",
        "EnvironmentalRelease",
        "ConfinedSpaceEntry",
        "LotoViolation",
        "HotWorkDeviation",
        "FleetIncident",
        "ExplosionRisk",
    };

    /// <summary>Workflow templates for every event code in <see cref="TemplatedEventCodes"/>, keyed by <see cref="EventDefinition.EventCode"/>.</summary>
    public static IReadOnlyDictionary<string, EventWorkflowTemplate> ByEventCode { get; } = TemplatedEventCodes
        .Select(eventCode => EventCatalog.FindByCode(eventCode)
            ?? throw new InvalidOperationException($"Event code '{eventCode}' was not found in EventCatalog.All."))
        .ToDictionary(
            eventDefinition => eventDefinition.EventCode,
            eventDefinition => new EventWorkflowTemplate(eventDefinition.EventCode, BuildSteps(eventDefinition)),
            StringComparer.Ordinal);

    /// <summary>Builds the uniform 5-step workflow for the given event type.</summary>
    public static IReadOnlyList<WorkflowStep> BuildSteps(EventDefinition eventDefinition)
    {
        string regulatorList = string.Join(", ", eventDefinition.RelatedRegulators.Select(regulator => regulator.Code()));
        string primarySop = eventDefinition.RequiredSops.Count > 0 ? eventDefinition.RequiredSops[0].Title : "the applicable SOP";
        string lowerTitle = eventDefinition.Title.ToLowerInvariant();

        return new[]
        {
            new WorkflowStep(1, "Notify", $"Notify the facility safety officer and {regulatorList} as required."),
            new WorkflowStep(2, "Contain", $"Execute containment/control procedures per {primarySop}."),
            new WorkflowStep(3, "Investigate", $"Investigate the root cause of the {lowerTitle}."),
            new WorkflowStep(4, "Remediate", "Complete corrective actions for any related compliance elements found missing at this facility."),
            new WorkflowStep(5, "ReportAndClose", $"File required regulatory documentation and close out the {lowerTitle} record."),
        };
    }
}
