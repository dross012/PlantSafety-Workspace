// ============================================================================
//  Project:        AI.Reasoning
//  Module:         Abstractions.Context
//  File:           SafetyReasoningInput.cs
//  Created:        2026-08-08
//  Description:    Phase E.1 (Path E: AI-Driven Safety Automation): the raw
//                  signal set SafetyReasoningEngine reasons over, spanning
//                  every dimension Path E's spec names. Reuses real
//                  vocabularies from Paths B/C wherever one exists
//                  (FacilityLifecycleStatus, HazardType, SeverityLevel,
//                  RegulatoryJurisdiction, EnterpriseWorkflowState) rather
//                  than inventing parallel ones. The Enterprise graph (D.1)
//                  and digital twin (D.7) dimensions are summarized as
//                  plain scalars (a connectivity count and a simulated
//                  severity) rather than full object graphs -- mirrors
//                  Enterprise.RulesEngine's Phase D.5 precedent of
//                  reasoning over flat signals, not deep domain references.
//                  "AI" here means deterministic, explicit, rule-based
//                  inference, per the program's standing "deterministic
//                  methods only" instruction -- no model calls, no
//                  randomness.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Lifecycle;
using Shared.FacilityDomain.Abstractions.Regulatory;
using Shared.FacilityDomain.Abstractions.Severity;

using Enterprise.WorkflowOrchestration.Abstractions.States;

namespace AI.Reasoning.Abstractions.Context;

/// <summary>The raw signal set a safety reasoning session evaluates against.</summary>
public sealed record SafetyReasoningInput(
    FacilityLifecycleStatus FacilityStatus,
    int EmployeeInjuryCount,
    int MachineFailureCount,
    HazardType HazardType,
    SeverityLevel SeverityLevel,
    IReadOnlyList<RegulatoryJurisdiction> ApplicableJurisdictions,
    EnterpriseWorkflowState WorkflowState,
    int EnterpriseGraphConnectedFacilityCount,
    SeverityLevel DigitalTwinSimulatedSeverity);
