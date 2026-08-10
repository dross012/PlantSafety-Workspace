// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Telemetry
//  File:           AuthenticationTelemetry.cs
//  Created:        2026-08-10
//  Description:    Phase 10 deliverable. Records a CallerContextResolution's
//                  outcome via PlantSafetyMetrics.ApiKeyResolutionsPerformed
//                  and a structured API_KEY_RESOLUTION_PERFORMED log event.
//                  Called once per request, by ApiKeyAuthenticationMiddleware.
// ============================================================================
using Shared.Authorization.Abstractions.Identity;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.Authorization.Infrastructure.Telemetry;

/// <summary>Records API-key resolution telemetry.</summary>
public static class AuthenticationTelemetry
{
    /// <summary>Records <paramref name="resolution"/>'s outcome as a metric increment and a structured log event.</summary>
    public static void RecordResolution(CallerContextResolution resolution, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(resolution);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        PlantSafetyMetrics.ApiKeyResolutionsPerformed.Add(1, new KeyValuePair<string, object?>("outcome", resolution.Outcome.ToString()));

        logEventWriter.Write(new LogEvent(
            EventId: "API_KEY_RESOLUTION_PERFORMED",
            Category: LogCategory.Authentication,
            Severity: LogSeverity.Info,
            Message: $"API-key resolution outcome: {resolution.Outcome} (resolved role \"{resolution.Caller.Role}\").",
            Payload: new Dictionary<string, object?>
            {
                ["Outcome"] = resolution.Outcome.ToString(),
                ["Role"] = resolution.Caller.Role.ToString(),
            },
            CorrelationId: resolution.Caller.CorrelationId,
            Module: "Authentication",
            OccurredAtUtc: clock.UtcNow));
    }
}
