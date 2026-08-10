// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Telemetry
//  File:           AuthorizationTelemetry.cs
//  Created:        2026-08-10
//  Description:    Phase 9 deliverable. Records an AuthorizationDecision as
//                  a metric increment (plus a denial increment, and an MFA
//                  requirement-failure increment, when applicable) and a
//                  structured AUTHORIZATION_CHECK_PERFORMED log event.
//                  Severity is Warning on denial, Info on allow -- every
//                  downstream *Authorization.cs test asserts this exact
//                  contract.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Checks;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.Authorization.Infrastructure.Telemetry;

/// <summary>Records authorization-check telemetry.</summary>
public static class AuthorizationTelemetry
{
    /// <summary>Records <paramref name="decision"/> as a metric increment and a structured log event.</summary>
    public static void RecordDecision(AuthorizationDecision decision, CallerContext caller, string module, string verb, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(decision);
        ArgumentNullException.ThrowIfNull(caller);
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(verb);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        PlantSafetyMetrics.AuthorizationChecksPerformed.Add(1, new KeyValuePair<string, object?>("module", module), new KeyValuePair<string, object?>("outcome", decision.IsAuthorized ? "Allow" : "Deny"));

        if (!decision.IsAuthorized)
        {
            PlantSafetyMetrics.AuthorizationDenials.Add(1, new KeyValuePair<string, object?>("module", module), new KeyValuePair<string, object?>("permission", $"{module}.{verb}"));
        }

        if (decision.MfaRequirementFailed)
        {
            PlantSafetyMetrics.MfaRequirementFailures.Add(1, new KeyValuePair<string, object?>("module", module));
        }

        logEventWriter.Write(new LogEvent(
            EventId: "AUTHORIZATION_CHECK_PERFORMED",
            Category: LogCategory.Authorization,
            Severity: decision.IsAuthorized ? LogSeverity.Info : LogSeverity.Warning,
            Message: $"Authorization check for \"{module}.{verb}\" by role \"{caller.Role}\": {(decision.IsAuthorized ? "allowed" : "denied")} -- {decision.Reason}",
            Payload: new Dictionary<string, object?>
            {
                ["Module"] = module,
                ["Verb"] = verb,
                ["Role"] = caller.Role.ToString(),
                ["IsAuthorized"] = decision.IsAuthorized,
                ["Reason"] = decision.Reason,
                ["MfaRequirementFailed"] = decision.MfaRequirementFailed,
            },
            CorrelationId: caller.CorrelationId,
            Module: module,
            OccurredAtUtc: clock.UtcNow));
    }
}
