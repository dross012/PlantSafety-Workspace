// ============================================================================
//  Project:        AI.SafetyCopilot
//  Module:         Abstractions.Engine
//  File:           SafetyCopilotEngine.cs
//  Created:        2026-08-08
//  Description:    Phase E.8 (Path E: AI-Driven Safety Automation):
//                  synthesizes audience-appropriate guidance by composing
//                  Path E's other four engines' outputs -- no hazard/
//                  workflow/regulatory/severity logic is reimplemented
//                  here. Employees get the actions that directly protect
//                  them (required actions, evacuation); supervisors get
//                  the actions that require their authority (SOP/permit/
//                  shutdown-restart). Every audience gets a hazard
//                  explanation, regulatory guidance, and a severity
//                  assessment summary, worded for their role. Pure
//                  function, no side effects, no reflection.
// ============================================================================
using AI.HazardUnderstanding.Abstractions.Semantics;
using AI.RegulatoryInterpretation.Abstractions.Interpretation;
using AI.RegulatoryInterpretation.Abstractions.Rules;
using AI.SafetyCopilot.Abstractions.Copilot;
using AI.SafetyCopilot.Abstractions.Recommendations;
using AI.SeverityAssessment.Abstractions.Assessment;
using AI.WorkflowAutomation.Abstractions.Automation;

namespace AI.SafetyCopilot.Abstractions.Engine;

/// <summary>Synthesizes audience-appropriate safety guidance by composing Path E's other engines' outputs.</summary>
public static class SafetyCopilotEngine
{
    /// <summary>Generates guidance for the requested audience from the given composed engine outputs.</summary>
    public static SafetyCopilotOutput Guide(SafetyCopilotInput input, DateTime? nowUtc = null)
    {
        DateTime generatedAtUtc = nowUtc ?? DateTime.UtcNow;

        IReadOnlyList<SafetyCopilotAction> actions = input.Audience == CopilotAudience.Employee
            ? BuildEmployeeActions(input.WorkflowAutomation)
            : BuildSupervisorActions(input.WorkflowAutomation);

        var recommendations = new List<SafetyCopilotRecommendation>
        {
            BuildHazardExplanation(input.HazardUnderstanding.Model, input.Audience),
            BuildSeveritySummary(input.SeverityAssessment, input.Audience),
        };
        recommendations.AddRange(BuildRegulatoryGuidance(input.RegulatoryInterpretation, input.Audience));

        return new SafetyCopilotOutput(actions, recommendations, generatedAtUtc);
    }

    private static IReadOnlyList<SafetyCopilotAction> BuildEmployeeActions(WorkflowAutomationOutput workflow)
    {
        var actions = new List<SafetyCopilotAction>();
        actions.AddRange(workflow.RequiredActions.Select(a => new SafetyCopilotAction(a.ActionCode, a.Description, CopilotAudience.Employee)));
        actions.AddRange(workflow.EvacuationSteps.Select(e => new SafetyCopilotAction("EVACUATE", e.Reason, CopilotAudience.Employee)));
        return actions;
    }

    private static IReadOnlyList<SafetyCopilotAction> BuildSupervisorActions(WorkflowAutomationOutput workflow)
    {
        var actions = new List<SafetyCopilotAction>();
        actions.AddRange(workflow.SopSteps.Select(s => new SafetyCopilotAction(s.SopCode, s.TriggerReason, CopilotAudience.Supervisor)));
        actions.AddRange(workflow.PermitRequirements.Select(p => new SafetyCopilotAction(p.PermitCode, p.TriggerReason, CopilotAudience.Supervisor)));
        actions.AddRange(workflow.ShutdownRestartSequence.Select(s => new SafetyCopilotAction(s.Code, s.Description, CopilotAudience.Supervisor)));
        return actions;
    }

    private static SafetyCopilotRecommendation BuildHazardExplanation(HazardSemanticModel model, CopilotAudience audience)
    {
        string guidance = audience == CopilotAudience.Employee
            ? $"This is a {model.HazardType} hazard ({model.Category}). Follow evacuation and required-action guidance."
            : $"{model.HazardType} hazard classified as {model.Category}. Related hazards to monitor: {string.Join(", ", model.RelatedHazards)}. Propagation: {string.Join(" -> ", model.PropagationChain)}.";

        return new SafetyCopilotRecommendation("Hazard Explanation", guidance, audience);
    }

    private static SafetyCopilotRecommendation BuildSeveritySummary(SeverityAssessmentOutput assessment, CopilotAudience audience)
    {
        string guidance = audience == CopilotAudience.Employee
            ? $"Assessed severity: {assessment.FinalAssessedSeverity}. Follow all instructions from supervisors and emergency personnel."
            : $"Assessed severity: {assessment.FinalAssessedSeverity} (base: {assessment.BaseSeverity}, digital twin: {assessment.DigitalTwinSimulatedSeverity}). Notes: {string.Join("; ", assessment.Notes)}";

        return new SafetyCopilotRecommendation("Severity Assessment", guidance, audience);
    }

    private static IReadOnlyList<SafetyCopilotRecommendation> BuildRegulatoryGuidance(RegulatoryInterpretationOutput interpretation, CopilotAudience audience)
    {
        if (audience == CopilotAudience.Employee)
        {
            return interpretation.Interpretations.Count == 0
                ? []
                : [new SafetyCopilotRecommendation("Regulatory Guidance", "This event may have regulatory reporting implications; notify your supervisor.", audience)];
        }

        return interpretation.Interpretations
            .Select(i => new SafetyCopilotRecommendation($"Regulatory Guidance ({i.Jurisdiction})", $"{i.Guidance} (report within {i.ReportingWindowHours}h)", audience))
            .ToArray();
    }
}
