// ============================================================================
//  Project:        AI.EnterpriseOrchestration
//  Module:         Abstractions.Orchestration
//  File:           FacilityOrchestrationContext.cs
//  Created:        2026-08-08
//  Description:    Phase E.9 (Path E: AI-Driven Safety Automation): one
//                  facility's already-computed Path E engine outputs, as
//                  fed into an enterprise-wide orchestration session.
//                  EnterpriseOrchestrationEngine aggregates across many of
//                  these rather than re-deriving any of them -- this is
//                  the "across facilities" composition layer Path D.8's
//                  multi-facility tenant registry made possible but never
//                  itself performed reasoning over.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Severity;

using AI.HazardUnderstanding.Abstractions.Semantics;
using AI.PredictiveOptimization.Abstractions.Optimization;
using AI.RegulatoryInterpretation.Abstractions.Interpretation;
using AI.WorkflowAutomation.Abstractions.Automation;

namespace AI.EnterpriseOrchestration.Abstractions.Orchestration;

/// <summary>One facility's already-computed Path E engine outputs.</summary>
public sealed record FacilityOrchestrationContext(
    Guid FacilityId,
    WorkflowAutomationOutput Workflow,
    HazardUnderstandingOutput Hazard,
    RegulatoryInterpretationOutput Regulatory,
    PredictiveOptimizationOutput Predictive,
    SeverityLevel DigitalTwinSimulatedSeverity);
