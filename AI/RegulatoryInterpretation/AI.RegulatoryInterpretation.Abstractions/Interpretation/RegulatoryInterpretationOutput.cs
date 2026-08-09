// ============================================================================
//  Project:        AI.RegulatoryInterpretation
//  Module:         Abstractions.Interpretation
//  File:           RegulatoryInterpretationOutput.cs
//  Created:        2026-08-08
//  Description:    Phase E.4: the result of a regulatory-interpretation
//                  session -- one RegulatoryInterpretationRule per
//                  applicable jurisdiction.
// ============================================================================
using AI.RegulatoryInterpretation.Abstractions.Rules;

namespace AI.RegulatoryInterpretation.Abstractions.Interpretation;

/// <summary>The result of a regulatory-interpretation session.</summary>
public sealed record RegulatoryInterpretationOutput(
    IReadOnlyList<RegulatoryInterpretationRule> Interpretations,
    DateTime GeneratedAtUtc);
