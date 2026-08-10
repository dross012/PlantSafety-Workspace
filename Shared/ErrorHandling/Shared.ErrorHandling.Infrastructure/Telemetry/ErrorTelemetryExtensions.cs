// ============================================================================
//  Project:        Shared.ErrorHandling.Infrastructure
//  Module:         Telemetry
//  File:           ErrorTelemetryExtensions.cs
//  Created:        2026-08-10
//  Description:    Phase 12 deliverable. Extension method that turns an
//                  ErrorEnvelope into a structured log payload with
//                  deterministic (alphabetical) key ordering -- reused by
//                  callers that want to log an envelope's shape without
//                  duplicating GlobalErrorHandler's own inline payload
//                  construction (Phase 2.3/2.4, unchanged).
// ============================================================================
using Shared.ErrorHandling.Abstractions.Envelope;

namespace Shared.ErrorHandling.Infrastructure.Telemetry;

/// <summary>Extension methods for emitting an ErrorEnvelope's shape through structured logs.</summary>
public static class ErrorTelemetryExtensions
{
    /// <summary>Converts <paramref name="envelope"/> into a structured log payload, keys in alphabetical order.</summary>
    public static IReadOnlyDictionary<string, object?> ToLogPayload(this ErrorEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        return new Dictionary<string, object?>
        {
            ["Category"] = envelope.Category.ToString(),
            ["CorrelationId"] = envelope.CorrelationId,
            ["ErrorCode"] = envelope.ErrorCode,
            ["Module"] = envelope.Module,
            ["OccurredAtUtc"] = envelope.OccurredAtUtc,
        };
    }
}
