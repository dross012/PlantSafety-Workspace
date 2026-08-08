// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Eventing
//  Layer:          Abstractions
//  File:           EventDefinition.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Immutable schema for a single recognized event TYPE (a
//                  generic classification -- "what does a chemical spill
//                  generically imply", not a specific occurrence). Instances
//                  live in EventCatalog.cs, one per real-world event type
//                  PlantSafety recognizes.
//
//                  ARCHITECTURAL SCOPE (see also EventRoutingEngine.cs):
//                  This subsystem is NOT a second event system. It does not
//                  publish events, does not persist events, does not define
//                  event aggregates, and never subscribes to Shared.EventBus.
//                  PlantSafety's real event domains (AccidentManagement,
//                  SafetyEvents/CrisisManagement/EmergencyManagement/
//                  DrillsAndExercises) remain the sole source of truth for
//                  event lifecycle, persistence, and bus publishing/
//                  subscription. This subsystem is a pure, stateless
//                  INTERPRETATION layer: given an event type code and a
//                  FacilityProfile, it computes what that event type implies
//                  for compliance, programs, SOPs, training, audits, permits,
//                  dashboards, and risk -- "what does it mean", never "what
//                  happened" or "what should the platform do about it at the
//                  infrastructure level". A real event domain (e.g.
//                  AccidentManagement) is expected to call into this layer
//                  (IEventRoutingService/IEventImpactService/
//                  IEventWorkflowService) as a consumer, the same way it
//                  might consult any other read-only reference/compute
//                  service -- this subsystem never becomes the owner or
//                  publisher of events.
//
//                  DERIVATION: every EventDefinition's RelatedHazards/
//                  RelatedPrograms/RequiredSOPs/RequiredTraining/
//                  RequiredPermits are computed from a single (SafetyFlag?,
//                  Regulator) source pairing per event, via the existing
//                  HazardCategoryCatalog/ProgramCatalog/ProgramSopRequirements/
//                  ProgramTrainingRequirements/ProgramPermitRequirements --
//                  never hand-authored. When an event has no specific
//                  SafetyFlag (e.g. "Near miss", "Injury", "OSHA recordable"
//                  -- generic events not tied to one hazard type),
//                  RelatedHazards/RelatedPrograms/RequiredSOPs/
//                  RequiredTraining/RequiredPermits are correctly empty at
//                  the catalog level: the generic event type alone does not
//                  imply a specific program until a real occurrence is
//                  further classified. See EventCatalog.cs for the full
//                  20-event source mapping and rationale.
//                  Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.Consumers;

namespace Shared.SafetyData.Abstractions.Models.Industries.Eventing;

/// <summary>Relative severity of an event type.</summary>
public enum EventSeverity
{
    /// <summary>Minor, no immediate harm or regulatory exposure (e.g. a near miss).</summary>
    Low,

    /// <summary>Moderate -- typically implies a required corrective action but not an emergency.</summary>
    Medium,

    /// <summary>Serious -- typically implies injury, environmental harm, or a significant regulatory obligation.</summary>
    High,

    /// <summary>Severe -- typically implies life-safety risk, major environmental/security consequence, or an imminent regulatory enforcement risk.</summary>
    Critical,
}

/// <summary>Broad classification of an event type.</summary>
public enum EventCategory
{
    /// <summary>Worker/workplace physical-safety event.</summary>
    Safety,

    /// <summary>Environmental release/waste/emissions event.</summary>
    Environmental,

    /// <summary>Regulatory filing/reporting/compliance-violation event.</summary>
    Regulatory,

    /// <summary>Physical or cyber security event.</summary>
    Security,
}

/// <summary>
/// A single recognized event type PlantSafety interprets -- a generic classification, not a
/// specific occurrence.
/// </summary>
/// <param name="EventCode">Stable, machine-referenceable event type code (e.g. "ChemicalSpill").</param>
/// <param name="Title">Human-readable event type title.</param>
/// <param name="Description">Human-readable description of what this event type represents.</param>
/// <param name="Severity">The event type's relative severity.</param>
/// <param name="Category">The event type's broad classification.</param>
/// <param name="RelatedHazards">Hazard categories this event type is generically associated with (empty if not tied to a specific <see cref="SafetyFlag"/>).</param>
/// <param name="RelatedPrograms">Safety Programs this event type generically implicates (empty if not tied to a specific <see cref="SafetyFlag"/>).</param>
/// <param name="RelatedRegulators">The regulator(s) this event type touches.</param>
/// <param name="RequiredSops">SOPs generically implicated by this event type (see <paramref name="RelatedPrograms"/>).</param>
/// <param name="RequiredTraining">Training topics generically implicated by this event type.</param>
/// <param name="RequiredPermits">Permit types generically implicated by this event type.</param>
public sealed record EventDefinition(
    string EventCode,
    string Title,
    string Description,
    EventSeverity Severity,
    EventCategory Category,
    IReadOnlyList<HazardCategory> RelatedHazards,
    IReadOnlyList<ProgramDefinition> RelatedPrograms,
    IReadOnlyList<Regulator> RelatedRegulators,
    IReadOnlyList<SopTemplate> RequiredSops,
    IReadOnlyList<TrainingRequirement> RequiredTraining,
    IReadOnlyList<PermitType> RequiredPermits);
