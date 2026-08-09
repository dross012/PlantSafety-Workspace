// ============================================================================
//  Project:        AI.Reasoning
//  Module:         Abstractions.Context
//  File:           SafetyReasoningOutput.cs
//  Created:        2026-08-08
//  Description:    Phase E.1: the result of a safety reasoning session --
//                  every conclusion whose rule matched, plus the worst-case
//                  severity across the input's own SeverityLevel and
//                  DigitalTwinSimulatedSeverity signals (mirrors the
//                  worst-case-wins aggregation convention used throughout
//                  this program, e.g. Shared.FacilityDomain's
//                  SeverityInferenceRules).
// ============================================================================
using Shared.FacilityDomain.Abstractions.Severity;

namespace AI.Reasoning.Abstractions.Context;

/// <summary>The result of a safety reasoning session.</summary>
public sealed record SafetyReasoningOutput(
    Guid ReasoningSessionId,
    IReadOnlyList<string> Conclusions,
    SeverityLevel OverallAssessedSeverity,
    DateTime GeneratedAtUtc);
