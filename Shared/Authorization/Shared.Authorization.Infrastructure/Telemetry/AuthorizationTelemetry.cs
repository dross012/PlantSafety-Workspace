// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Telemetry
//  File:           AuthorizationTelemetry.cs
//  Created:        2026-08-10
//  Description:    Phase 9 (Authorization Modernization) deliverable. Reports
//                  an AuthorizationDecision through the existing Phase 8
//                  Shared.Telemetry primitives (ILogEventWriter/LogEvent,
//                  PlantSafetyMetrics) -- no new logging, metrics, or tracing
//                  primitive is introduced. One structured
//                  AUTHORIZATION_CHECK_PERFORMED log event per check
//                  (Severity=Info on allow, Warning on deny), plus the
//                  AuthorizationChecksPerformed/AuthorizationDenials/
//                  MfaRequirementFailures counters added to PlantSafetyMetrics
//                  in this same phase.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Checks;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.Authorization.Infrastructure.Telemetry;

/// <summary>Reports an AuthorizationDecision through Shared.Telemetry's existing logging and metrics primitives.</summary>
public static class AuthorizationTelemetry
{
    /// <summary>
    /// Records <paramref name="decision"/> for a check against <paramref name="module"/>/<paramref name="requiredPermission"/>:
    /// increments AuthorizationChecksPerformed (and AuthorizationDenials on deny), and writes an
    /// AUTHORIZATION_CHECK_PERFORMED log event carrying the caller's role and the decision's reason.
    /// </summary>
    public static void RecordDecision(
        AuthorizationDecision decision,
        CallerContext caller,
        string module,
        string requiredPermission,
        IClock clock,
        ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(decision);
        ArgumentNullException.ThrowIfNull(caller);

        string outcome = decision.IsAuthorized ? "Allow" : "Deny";
        PlantSafetyMetrics.AuthorizationChecksPerformed.Add(1,
            new KeyValuePair<string, object?>("module", module),
            new KeyValuePair<string, object?>("outcome", outcome));

        if (!decision.IsAuthorized)
        {
            PlantSafetyMetrics.AuthorizationDenials.Add(1,
                new KeyValuePair<string, object?>("module", module),
                new KeyValuePair<string, object?>("permission", requiredPermission));
        }

        logEventWriter.Write(new LogEvent(
            EventId: "AUTHORIZATION_CHECK_PERFORMED",
            Category: LogCategory.Authorization,
            Severity: decision.IsAuthorized ? LogSeverity.Info : LogSeverity.Warning,
            Message: $"Authorization check for \"{module}.{requiredPermission}\" by role \"{caller.Role.Name}\": {outcome}. {decision.Reason}",
            Payload: new Dictionary<string, object?>
            {
                ["Module"] = module,
                ["RequiredPermission"] = requiredPermission,
                ["CallerRole"] = caller.Role.Name,
                ["Outcome"] = outcome,
                ["Reason"] = decision.Reason,
            },
            CorrelationId: caller.CorrelationId,
            Module: "Authorization",
            OccurredAtUtc: clock.UtcNow));
    }

    /// <summary>Records an MFA requirement failure: increments MfaRequirementFailures and writes an MFA_REQUIREMENT_NOT_MET log event.</summary>
    public static void RecordMfaFailure(CallerContext caller, string module, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(caller);

        PlantSafetyMetrics.MfaRequirementFailures.Add(1, new KeyValuePair<string, object?>("module", module));

        logEventWriter.Write(new LogEvent(
            EventId: "MFA_REQUIREMENT_NOT_MET",
            Category: LogCategory.Authorization,
            Severity: LogSeverity.Warning,
            Message: $"MFA required for module \"{module}\" but caller's MfaStatus is \"{caller.MfaStatus}\".",
            Payload: new Dictionary<string, object?> { ["Module"] = module, ["MfaStatus"] = caller.MfaStatus.ToString() },
            CorrelationId: caller.CorrelationId,
            Module: "Authorization",
            OccurredAtUtc: clock.UtcNow));
    }
}
