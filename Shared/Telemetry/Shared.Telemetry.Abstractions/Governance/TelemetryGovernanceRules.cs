// ============================================================================
//  Project:        Shared.Telemetry
//  Module:         Abstractions.Governance
//  File:           TelemetryGovernanceRules.cs
//  Created:        2026-08-10
//  Description:    Phase 8 (Observability, Telemetry, Diagnostics, and
//                  Governance) deliverable. Pure validation over telemetry
//                  shapes already in use across this program -- no
//                  orchestration/scheduling/routing/diagnostics governance
//                  is implemented here, since none of those have any real
//                  execution path in this program to govern (Phase 7 §0,
//                  Phase 8 scoping). Every rule here is a pure function:
//                  input in, violation list out, zero side effects.
//                  Violations are reported through the caller (see
//                  TelemetryGovernanceEvaluator below) as an ordinary
//                  LogEvent -- not a new event type -- and a
//                  PlantSafetyMetrics.GovernanceViolationsDetected
//                  increment, reusing existing primitives exactly as every
//                  other Phase 8 file in this program does.
// ============================================================================
using System.Text.RegularExpressions;

using Shared.Telemetry.Abstractions.Events;

namespace Shared.Telemetry.Abstractions.Governance;

/// <summary>Pure validation rules for telemetry shapes: log events, metric names, and correlation propagation.</summary>
public static partial class TelemetryGovernanceRules
{
    // plantsafety.snake_case_name -- matches every metric name already defined in PlantSafetyMetrics.
    [GeneratedRegex(@"^plantsafety\.[a-z][a-z0-9_]*$")]
    private static partial Regex MetricNamePattern();

    // UPPER_SNAKE_CASE -- matches every EventId literal already used across this program
    // (e.g. "FACILITY_CREATED", "RELIABILITY_OPERATION_FAILED").
    [GeneratedRegex(@"^[A-Z][A-Z0-9_]*$")]
    private static partial Regex EventIdPattern();

    /// <summary>Validates a log event's shape. Returns every violation found (empty if valid).</summary>
    public static IReadOnlyList<string> ValidateLogEvent(ILogEvent logEvent)
    {
        ArgumentNullException.ThrowIfNull(logEvent);

        var violations = new List<string>();

        if (string.IsNullOrWhiteSpace(logEvent.EventId))
        {
            violations.Add("EventId is missing or empty.");
        }
        else if (!EventIdPattern().IsMatch(logEvent.EventId))
        {
            violations.Add($"EventId \"{logEvent.EventId}\" does not match the required UPPER_SNAKE_CASE format.");
        }

        if (string.IsNullOrWhiteSpace(logEvent.CorrelationId))
        {
            violations.Add("CorrelationId is missing or empty -- correlation propagation is broken for this event.");
        }

        if (string.IsNullOrWhiteSpace(logEvent.Module))
        {
            violations.Add("Module is missing or empty.");
        }

        if (string.IsNullOrWhiteSpace(logEvent.Message))
        {
            violations.Add("Message is missing or empty -- structured logging still requires a human-readable summary.");
        }

        if (logEvent.OccurredAtUtc == default)
        {
            violations.Add("OccurredAtUtc is unset (default DateTime) -- every event must carry a real, deterministic-clock-sourced timestamp.");
        }

        return violations;
    }

    /// <summary>Validates a metric name against this program's established "plantsafety.snake_case_name" convention.</summary>
    public static IReadOnlyList<string> ValidateMetricName(string metricName)
    {
        ArgumentNullException.ThrowIfNull(metricName);

        var violations = new List<string>();
        if (string.IsNullOrWhiteSpace(metricName))
        {
            violations.Add("Metric name is missing or empty.");
        }
        else if (!MetricNamePattern().IsMatch(metricName))
        {
            violations.Add($"Metric name \"{metricName}\" does not match the required \"plantsafety.snake_case_name\" convention.");
        }

        return violations;
    }

    /// <summary>Validates that a correlation ID is present and non-empty -- the minimum bar for correlation propagation correctness.</summary>
    public static IReadOnlyList<string> ValidateCorrelationPropagation(string? correlationId)
    {
        var violations = new List<string>();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            violations.Add("CorrelationId was not propagated to this operation.");
        }

        return violations;
    }
}
