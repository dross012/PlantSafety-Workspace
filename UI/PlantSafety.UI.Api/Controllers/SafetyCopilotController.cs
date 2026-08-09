// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           SafetyCopilotController.cs
//  Created:        2026-08-09
//  Description:    "Run this engine" surface over AI.SafetyCopilot's pure
//                  deterministic SafetyCopilotEngine.Guide (Phase E.8).
//                  SafetyCopilotInput normally requires four already-computed
//                  engine outputs (HazardUnderstanding/WorkflowAutomation/
//                  RegulatoryInterpretation/SeverityAssessment) -- for a
//                  usable single-screen UI experience, this controller
//                  accepts one flat set of primitive signals and internally
//                  composes all four upstream engines (empty rule sets,
//                  same documented limitation as their own dedicated
//                  controllers) before calling SafetyCopilotEngine.Guide.
//                  No new reasoning is introduced; this is pure composition,
//                  matching Phase E.8's own "zero new reasoning" design.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Propagation;
using Shared.FacilityDomain.Abstractions.Severity;
using Shared.WorkflowEngine.Abstractions.Engine;
using Shared.WorkflowEngine.Abstractions.Triggers;

using AI.HazardUnderstanding.Abstractions.Engine;
using AI.HazardUnderstanding.Abstractions.Semantics;
using AI.RegulatoryInterpretation.Abstractions.Engine;
using AI.RegulatoryInterpretation.Abstractions.Interpretation;
using AI.SafetyCopilot.Abstractions.Copilot;
using AI.SafetyCopilot.Abstractions.Engine;
using AI.SeverityAssessment.Abstractions.Assessment;
using AI.SeverityAssessment.Abstractions.Engine;
using AI.SeverityAssessment.Abstractions.Rules;
using AI.WorkflowAutomation.Abstractions.Automation;
using AI.WorkflowAutomation.Abstractions.Engine;
using AI.WorkflowAutomation.Abstractions.Rules;

using Shared.Deterministic.Abstractions.Clock;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>The single flat request the Copilot dashboard page submits; composed internally into the four upstream engines' inputs.</summary>
public sealed record SafetyCopilotGuidanceRequest(
    CopilotAudience Audience,
    HazardType HazardType,
    PropagationStage OriginPropagationStage,
    SeverityLevel SeverityLevel,
    EvacuationScope AffectedScope,
    int EmployeeInjuryCount,
    int EmployeeExposureCount,
    int MachineFailureCount,
    int EventChainLength,
    SeverityLevel DigitalTwinSimulatedSeverity);

/// <summary>Runs AI.SafetyCopilot's deterministic audience-aware guidance, composing the four upstream Path E engines.</summary>
[ApiController]
[Route("api/v1/ui/engines/safety-copilot")]
public sealed class SafetyCopilotController(IClock clock) : ControllerBase
{
    /// <summary>POST api/v1/ui/engines/safety-copilot/guide -- composes Path E's engines and returns tailored guidance.</summary>
    [HttpPost("guide")]
    public ActionResult<SafetyCopilotOutput> Guide([FromBody] SafetyCopilotGuidanceRequest request)
    {
        DateTime now = clock.UtcNow;

        HazardUnderstandingOutput hazard = HazardUnderstandingEngine.Understand(
            new HazardUnderstandingInput(request.HazardType, request.OriginPropagationStage, request.EmployeeInjuryCount, request.MachineFailureCount, request.EventChainLength),
            now);

        var workflowContext = new WorkflowContext(request.HazardType, request.SeverityLevel, request.AffectedScope, request.EmployeeExposureCount, request.MachineFailureCount, request.EventChainLength);
        WorkflowAutomationOutput workflow = WorkflowAutomationEngine.Automate(
            new WorkflowAutomationInput(workflowContext, RequiresShutdown: false, RequiresRestart: false),
            AI.WorkflowAutomation.Abstractions.Rules.WorkflowAutomationRuleSet.Standard,
            now);

        RegulatoryInterpretationOutput regulatory = RegulatoryInterpretationEngine.Interpret(
            new RegulatoryInterpretationInput(request.HazardType, request.SeverityLevel), now);

        SeverityAssessmentOutput severity = SeverityAssessmentEngine.Assess(
            new SeverityAssessmentInput(request.HazardType, request.EmployeeInjuryCount, request.MachineFailureCount, request.EventChainLength, request.DigitalTwinSimulatedSeverity),
            SeverityAssessmentRuleSet.Standard,
            now);

        var copilotInput = new AI.SafetyCopilot.Abstractions.Copilot.SafetyCopilotInput(request.Audience, hazard, workflow, regulatory, severity);
        return Ok(SafetyCopilotEngine.Guide(copilotInput, now));
    }
}
