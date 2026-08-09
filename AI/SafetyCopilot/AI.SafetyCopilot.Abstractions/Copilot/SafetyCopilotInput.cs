// ============================================================================
//  Project:        AI.SafetyCopilot
//  Module:         Abstractions.Copilot
//  File:           SafetyCopilotInput.cs
//  Created:        2026-08-08
//  Description:    Phase E.8 (Path E: AI-Driven Safety Automation): the
//                  input SafetyCopilotEngine.Guide reasons over. The
//                  copilot is deliberately a composition layer, not a new
//                  reasoning engine -- it wraps the outputs of the four
//                  AI engines already built this path (HazardUnderstanding
//                  E.2, WorkflowAutomation E.3, RegulatoryInterpretation
//                  E.4, SeverityAssessment E.5) rather than re-deriving
//                  any of their conclusions.
// ============================================================================
using AI.HazardUnderstanding.Abstractions.Semantics;
using AI.RegulatoryInterpretation.Abstractions.Interpretation;
using AI.SeverityAssessment.Abstractions.Assessment;
using AI.WorkflowAutomation.Abstractions.Automation;

namespace AI.SafetyCopilot.Abstractions.Copilot;

/// <summary>The input a safety-copilot session reasons over -- the composed outputs of Path E's other engines.</summary>
public sealed record SafetyCopilotInput(
    CopilotAudience Audience,
    HazardUnderstandingOutput HazardUnderstanding,
    WorkflowAutomationOutput WorkflowAutomation,
    RegulatoryInterpretationOutput RegulatoryInterpretation,
    SeverityAssessmentOutput SeverityAssessment);
