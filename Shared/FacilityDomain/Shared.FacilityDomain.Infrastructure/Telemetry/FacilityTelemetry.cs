// ============================================================================
//  Project:        Shared.FacilityDomain.Infrastructure
//  Module:         Telemetry
//  File:           FacilityTelemetry.cs
//  Created:        2026-08-10
//  Description:    Phase 8 (Observability, Telemetry, Diagnostics, and
//                  Governance) deliverable. Wraps Facility's three
//                  deterministic inference-rule engines (HazardInferenceRules,
//                  SeverityInferenceRules, RegulatoryInferenceRules -- all
//                  unchanged by this phase, still pure functions) with
//                  logging, metrics, and reliability reporting.
//
//                  HONEST CAVEAT: none of these three rule engines has a
//                  live caller anywhere in PlantSafety.UI.Api today --
//                  confirmed by direct search before writing this file.
//                  FacilitiesController/FacilityService supply HazardType/
//                  SeverityLevel directly from API callers rather than
//                  inferring them; the only real callers of these rules are
//                  the deferred AI.* engines (HazardUnderstandingEngine,
//                  SeverityAssessmentEngine, RegulatoryInterpretationEngine).
//                  This wrapper exists so the rules are instrumentation-
//                  ready the moment a real caller is authorized to use
//                  them, per this phase's "instrument what exists, don't
//                  invent a caller" scope.
// ============================================================================
using Shared.Deterministic.Abstractions.Clock;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Regulatory;
using Shared.FacilityDomain.Abstractions.Severity;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;
using Shared.Telemetry.Abstractions.Reliability;

namespace Shared.FacilityDomain.Infrastructure.Telemetry;

/// <summary>Instruments Facility's deterministic inference-rule engines with deterministic logging, metrics, and reliability reporting.</summary>
public static class FacilityTelemetry
{
    private const string Module = "Facility";

    /// <summary>Infers a HazardType via HazardInferenceRules, recording a FacilityInferenceRuleEvaluations metric and a structured log event.</summary>
    public static async Task<HazardType> InferHazardTypeWithTelemetryAsync(
        string eventType,
        string? description,
        IClock clock,
        ILogEventWriter logEventWriter,
        string correlationId)
    {
        ArgumentNullException.ThrowIfNull(eventType);

        HazardType result = await ReliabilityHooks.ExecuteAsync(
            Module,
            "HazardInferenceRules.InferFromText",
            clock,
            logEventWriter,
            correlationId,
            () => Task.FromResult(HazardInferenceRules.InferHazardTypeFromSafetyEvent(eventType, description)));

        RecordRuleEvaluation(clock, logEventWriter, correlationId, ruleName: "Hazard", resultDescription: result.ToString());
        return result;
    }

    /// <summary>Infers a SeverityLevel via SeverityInferenceRules, recording a FacilityInferenceRuleEvaluations metric and a structured log event.</summary>
    public static async Task<SeverityLevel> InferSeverityWithTelemetryAsync(
        HazardType hazardType,
        int employeeInjuryCount,
        int machineFailureCount,
        int eventChainLength,
        IClock clock,
        ILogEventWriter logEventWriter,
        string correlationId)
    {
        SeverityLevel result = await ReliabilityHooks.ExecuteAsync(
            Module,
            "SeverityInferenceRules.Infer",
            clock,
            logEventWriter,
            correlationId,
            () => Task.FromResult(SeverityInferenceRules.Infer(hazardType, employeeInjuryCount, machineFailureCount, eventChainLength)));

        RecordRuleEvaluation(clock, logEventWriter, correlationId, ruleName: "Severity", resultDescription: result.ToString());
        return result;
    }

    /// <summary>Infers applicable RegulatoryJurisdictions via RegulatoryInferenceRules, recording a FacilityInferenceRuleEvaluations metric and a structured log event.</summary>
    public static async Task<IReadOnlyList<RegulatoryJurisdiction>> InferRegulatoryJurisdictionsWithTelemetryAsync(
        HazardType hazardType,
        SeverityLevel severityLevel,
        IClock clock,
        ILogEventWriter logEventWriter,
        string correlationId)
    {
        IReadOnlyList<RegulatoryJurisdiction> result = await ReliabilityHooks.ExecuteAsync(
            Module,
            "RegulatoryInferenceRules.Infer",
            clock,
            logEventWriter,
            correlationId,
            () => Task.FromResult(RegulatoryInferenceRules.Infer(hazardType, severityLevel)));

        RecordRuleEvaluation(clock, logEventWriter, correlationId, ruleName: "Regulatory", resultDescription: $"{result.Count} jurisdiction(s)");
        return result;
    }

    private static void RecordRuleEvaluation(IClock clock, ILogEventWriter logEventWriter, string correlationId, string ruleName, string resultDescription)
    {
        PlantSafetyMetrics.FacilityInferenceRuleEvaluations.Add(1, new KeyValuePair<string, object?>("rule", ruleName));

        logEventWriter.Write(new LogEvent(
            EventId: "FACILITY_INFERENCE_RULE_EVALUATED",
            Category: LogCategory.Facility,
            Severity: LogSeverity.Info,
            Message: $"Facility {ruleName} inference rule evaluated: {resultDescription}.",
            Payload: new Dictionary<string, object?> { ["Rule"] = ruleName, ["Result"] = resultDescription },
            CorrelationId: correlationId,
            Module: Module,
            OccurredAtUtc: clock.UtcNow));
    }
}
