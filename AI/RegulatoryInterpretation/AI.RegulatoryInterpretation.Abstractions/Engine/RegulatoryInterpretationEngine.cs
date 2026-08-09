// ============================================================================
//  Project:        AI.RegulatoryInterpretation
//  Module:         Abstractions.Engine
//  File:           RegulatoryInterpretationEngine.cs
//  Created:        2026-08-08
//  Description:    Phase E.4 (Path E: AI-Driven Safety Automation):
//                  determines which jurisdictions apply via Shared.
//                  FacilityDomain's real RegulatoryInferenceRules (Phase
//                  C.4, unchanged) and returns each one's deterministic
//                  interpretation via RegulatoryInterpretationCatalog.
//                  Pure function, no side effects, no reflection.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Regulatory;

using AI.RegulatoryInterpretation.Abstractions.Interpretation;
using AI.RegulatoryInterpretation.Abstractions.Rules;

namespace AI.RegulatoryInterpretation.Abstractions.Engine;

/// <summary>Deterministically interprets which regulatory jurisdictions apply and what each requires.</summary>
public static class RegulatoryInterpretationEngine
{
    /// <summary>Interprets every applicable jurisdiction for the given hazard/severity context.</summary>
    public static RegulatoryInterpretationOutput Interpret(RegulatoryInterpretationInput input, DateTime? nowUtc = null)
    {
        IReadOnlyList<RegulatoryJurisdiction> applicableJurisdictions = RegulatoryInferenceRules.Infer(input.HazardType, input.SeverityLevel);
        IReadOnlyList<RegulatoryInterpretationRule> interpretations = applicableJurisdictions
            .Select(RegulatoryInterpretationCatalog.InterpretationFor)
            .ToArray();

        return new RegulatoryInterpretationOutput(interpretations, nowUtc ?? DateTime.UtcNow);
    }
}
