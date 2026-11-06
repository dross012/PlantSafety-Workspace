// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Telemetry
//  File:           AuthenticationTelemetry.cs
//  Created:        2026-08-10
//  Description:    Phase 10 (Authentication Layer Modernization) deliverable.
//                  Reports a CallerContextResolution through the existing
//                  Phase 8 Shared.Telemetry primitives (ILogEventWriter/
//                  LogEvent, PlantSafetyMetrics) -- the same pattern
//                  AuthorizationTelemetry (Phase 9) established. No new
//                  logging, metrics, or tracing primitive is introduced; span
//                  ("tracing") and correlation concerns are the existing
//                  CorrelationId field on LogEvent, not a second mechanism --
//                  see this file's AuthenticationTracing/AuthenticationCorrelation
//                  companion below, which are thin, named wrappers over the
//                  same LogEvent/CorrelationId plumbing, not new primitives.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Identity;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.Authorization.Infrastructure.Telemetry;

/// <summary>Reports a CallerContextResolution through Shared.Telemetry's existing logging and metrics primitives.</summary>
public static class AuthenticationTelemetry
{
    /// <summary>
    /// Records <paramref name="resolution"/>: increments ApiKeyResolutionsPerformed (tagged by
    /// outcome), and writes one API_KEY_RESOLVED log event.
    /// </summary>
    public static void RecordResolution(CallerContextResolution resolution, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(resolution);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        ApiKeyIdentity identity = resolution.Identity;
        string outcome = !identity.KeyProvided ? "NoKeyProvided" : identity.KeyRecognized ? "Recognized" : "Unrecognized";

        PlantSafetyMetrics.ApiKeyResolutionsPerformed.Add(1, new KeyValuePair<string, object?>("outcome", outcome));

        logEventWriter.Write(new LogEvent(
            EventId: "API_KEY_RESOLVED",
            Category: LogCategory.Authentication,
            Severity: identity.KeyProvided && !identity.KeyRecognized ? LogSeverity.Warning : LogSeverity.Info,
            Message: $"API key resolution: {outcome}. Resolved role \"{resolution.Caller.Role.Name}\".",
            Payload: new Dictionary<string, object?>
            {
                ["Outcome"] = outcome,
                ["ResolvedRole"] = resolution.Caller.Role.Name,
            },
            CorrelationId: resolution.Caller.CorrelationId,
            Module: "Authentication",
            OccurredAtUtc: clock.UtcNow));
    }
}
