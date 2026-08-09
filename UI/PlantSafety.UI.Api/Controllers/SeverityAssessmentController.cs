// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           SeverityAssessmentController.cs
//  Created:        2026-08-09
//  Description:    "Run this engine" surface over AI.SeverityAssessment's
//                  pure deterministic SeverityAssessmentEngine.Assess (Phase
//                  E.5), evaluated against SeverityAssessmentRuleSet.Standard.
//                  Phase 2.9: the representative AI Engine Stabilization
//                  example -- wraps the call in Shared.AiInference's
//                  AiInferenceExecutor (timeout + fallback policy,
//                  inference envelope with deterministic input/output
//                  hashes). Fallback is fail-safe, not fail-open: if
//                  assessment itself fails, the fallback assumes
//                  Catastrophic severity rather than a lesser one --
//                  the correct default for a safety system.
// ============================================================================
using System.Text.Json;

using Microsoft.AspNetCore.Mvc;

using AI.SeverityAssessment.Abstractions.Assessment;
using AI.SeverityAssessment.Abstractions.Engine;
using AI.SeverityAssessment.Abstractions.Rules;

using Shared.AiInference.Abstractions.Envelope;
using Shared.AiInference.Abstractions.Policy;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Deterministic.Abstractions.Random;
using Shared.FacilityDomain.Abstractions.Severity;
using Shared.Telemetry.Abstractions.Correlation;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>The governed response: the real (or fallback) assessment plus its inference envelope.</summary>
public sealed record SeverityAssessmentInferenceResponse(SeverityAssessmentOutput Result, AiInferenceEnvelope Inference);

/// <summary>Runs AI.SeverityAssessment's deterministic severity assessment under AI inference policy.</summary>
[ApiController]
[Route("api/v1/ui/engines/severity-assessment")]
public sealed class SeverityAssessmentController(IClock clock, IRandomProvider randomProvider, ICorrelationContext correlationContext) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>POST api/v1/ui/engines/severity-assessment/assess -- assesses severity against the standard rule set, under timeout/fallback policy.</summary>
    [HttpPost("assess")]
    public async Task<ActionResult<SeverityAssessmentInferenceResponse>> Assess([FromBody] SeverityAssessmentInput input)
    {
        AiInferenceResult<SeverityAssessmentOutput> result = await AiInferenceExecutor.ExecuteAsync(
            input,
            i => SeverityAssessmentEngine.Assess(i, SeverityAssessmentRuleSet.Standard),
            fallback: i => new SeverityAssessmentOutput(
                SeverityLevel.Catastrophic,
                i.DigitalTwinSimulatedSeverity,
                SeverityLevel.Catastrophic,
                ["Fallback: severity assessment did not complete; assumed worst-case (Catastrophic) per fail-safe policy."],
                clock.UtcNow),
            AiInferencePolicy.Standard,
            modelName: "SeverityAssessmentEngine",
            correlationContext.CorrelationId,
            serializeInput: i => JsonSerializer.Serialize(i, JsonOptions),
            serializeOutput: o => JsonSerializer.Serialize(o, JsonOptions),
            clock,
            randomProvider);

        return Ok(new SeverityAssessmentInferenceResponse(result.Output, result.Envelope));
    }
}
