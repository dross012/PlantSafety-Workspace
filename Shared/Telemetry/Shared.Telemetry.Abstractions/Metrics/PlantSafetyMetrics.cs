// ============================================================================
//  Project:        Shared.Telemetry
//  Module:         Abstractions.Metrics
//  File:           PlantSafetyMetrics.cs
//  Created:        2026-08-09
//  Description:    Phase 3.3 deliverable: the one Meter every host in this
//                  repo publishes counters through. Built entirely on
//                  System.Diagnostics.Metrics (BCL, no NuGet package, no
//                  OTLP endpoint or Prometheus collector required) -- this
//                  sandbox has neither, so counters are observable today via
//                  `dotnet-counters monitor --process-id <pid> --counters
//                  PlantSafety` without standing up any collector, and the
//                  same Meter can be exported to a real collector later via
//                  OpenTelemetry.Extensions.Hosting's AddMeter("PlantSafety")
//                  without touching call sites.
// ============================================================================
using System.Diagnostics.Metrics;

namespace Shared.Telemetry.Abstractions.Metrics;

/// <summary>The shared Meter and counters every host publishes request/error/circuit-breaker metrics through.</summary>
public static class PlantSafetyMetrics
{
    /// <summary>The Meter name hosts register with an exporter (e.g. AddMeter("PlantSafety")).</summary>
    public const string MeterName = "PlantSafety";

    private static readonly Meter Meter = new(MeterName, "1.0");

    /// <summary>Total requests handled, tagged by module.</summary>
    public static readonly Counter<long> RequestsHandled = Meter.CreateCounter<long>(
        "plantsafety.requests_handled", unit: "{request}", description: "Total requests handled, tagged by module.");

    /// <summary>Total unhandled exceptions converted to ErrorEnvelope responses, tagged by module and error category.</summary>
    public static readonly Counter<long> ErrorsHandled = Meter.CreateCounter<long>(
        "plantsafety.errors_handled", unit: "{error}", description: "Total unhandled exceptions handled, tagged by module and error category.");

    /// <summary>Total times a circuit breaker recorded a failure that tripped it open.</summary>
    public static readonly Counter<long> CircuitBreakerTrips = Meter.CreateCounter<long>(
        "plantsafety.circuit_breaker_trips", unit: "{trip}", description: "Total circuit breaker trips, tagged by module.");

    // ---- Phase 8 (Observability, Telemetry, Diagnostics, and Governance): instrumentation for
    //      real execution paths only (WorkflowEngine.Evaluate, live via WorkflowEngineController)
    //      and dormant-but-real capabilities (Facility's inference rules, Audit's search --
    //      neither has a live caller in PlantSafety.UI.Api today; see
    //      Shared.FacilityDomain.Infrastructure/Telemetry/FacilityTelemetry.cs and
    //      Shared.AuditLogging.Infrastructure/Telemetry/AuditTelemetry.cs for that caveat). No
    //      new Meter, no new metric type beyond Counter/Histogram -- same Meter instance as
    //      every other instrument in this class. ----

    /// <summary>Total WorkflowEngine.Evaluate invocations, tagged by resulting SeverityLevel.</summary>
    public static readonly Counter<long> WorkflowDecisionsEvaluated = Meter.CreateCounter<long>(
        "plantsafety.workflow_decisions_evaluated", unit: "{decision}", description: "Total WorkflowEngine.Evaluate invocations, tagged by resulting SeverityLevel.");

    /// <summary>Total Facility inference-rule evaluations (Hazard/Severity/Regulatory), tagged by rule name.</summary>
    public static readonly Counter<long> FacilityInferenceRuleEvaluations = Meter.CreateCounter<long>(
        "plantsafety.facility_inference_rule_evaluations", unit: "{evaluation}", description: "Total Facility inference-rule evaluations, tagged by rule name.");

    /// <summary>Total IAuditLogRepository.SearchAsync invocations, tagged by outcome (Success/Failure).</summary>
    public static readonly Counter<long> AuditSearchesExecuted = Meter.CreateCounter<long>(
        "plantsafety.audit_searches_executed", unit: "{search}", description: "Total audit log searches executed, tagged by outcome.");

    /// <summary>Duration of each IAuditLogRepository.SearchAsync call, in milliseconds.</summary>
    public static readonly Histogram<double> AuditSearchDurationMs = Meter.CreateHistogram<double>(
        "plantsafety.audit_search_duration_ms", unit: "ms", description: "Duration of each audit log search, in milliseconds.");

    /// <summary>Total operations wrapped by ReliabilityHooks.ExecuteAsync that failed, tagged by module and operation name.</summary>
    public static readonly Counter<long> ReliabilityFailures = Meter.CreateCounter<long>(
        "plantsafety.reliability_failures", unit: "{failure}", description: "Total ReliabilityHooks-wrapped operations that failed, tagged by module and operation.");

    /// <summary>Duration of every operation wrapped by ReliabilityHooks.ExecuteAsync, in milliseconds.</summary>
    public static readonly Histogram<double> ReliabilityOperationDurationMs = Meter.CreateHistogram<double>(
        "plantsafety.reliability_operation_duration_ms", unit: "ms", description: "Duration of every ReliabilityHooks-wrapped operation, in milliseconds.");

    /// <summary>Total telemetry governance violations detected, tagged by rule name.</summary>
    public static readonly Counter<long> GovernanceViolationsDetected = Meter.CreateCounter<long>(
        "plantsafety.governance_violations_detected", unit: "{violation}", description: "Total telemetry governance violations detected, tagged by rule name.");

    // ---- Phase 9 (Authorization Modernization): CallerContext-based authorization checks
    //      (see Shared.Authorization.Abstractions.Checks.AuthorizationCheck), tagged by module
    //      and outcome. Same Meter, same pattern as every instrument above. ----

    /// <summary>Total authorization checks performed, tagged by module and outcome (Allow/Deny).</summary>
    public static readonly Counter<long> AuthorizationChecksPerformed = Meter.CreateCounter<long>(
        "plantsafety.authorization_checks_performed", unit: "{check}", description: "Total authorization checks performed, tagged by module and outcome.");

    /// <summary>Total authorization checks that denied the caller, tagged by module and required permission.</summary>
    public static readonly Counter<long> AuthorizationDenials = Meter.CreateCounter<long>(
        "plantsafety.authorization_denials", unit: "{denial}", description: "Total authorization checks that denied the caller, tagged by module and required permission.");

    /// <summary>Total MFA requirement checks that failed (caller's MfaStatus was not Verified), tagged by module.</summary>
    public static readonly Counter<long> MfaRequirementFailures = Meter.CreateCounter<long>(
        "plantsafety.mfa_requirement_failures", unit: "{failure}", description: "Total MFA requirement checks that failed, tagged by module.");

    // ---- Phase 10 (Authentication Layer Modernization): deterministic API-key resolution
    //      (see Shared.Authorization.Abstractions.Caller.CallerContextFactory), tagged by
    //      outcome. Same Meter, same pattern as every instrument above. ----

    /// <summary>Total API-key resolutions performed, tagged by outcome (NoKeyProvided/Recognized/Unrecognized).</summary>
    public static readonly Counter<long> ApiKeyResolutionsPerformed = Meter.CreateCounter<long>(
        "plantsafety.api_key_resolutions_performed", unit: "{resolution}", description: "Total API-key resolutions performed, tagged by outcome.");
}
