// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Middleware
//  File:           GlobalErrorHandler.cs
//  Created:        2026-08-09
//  Description:    Phase 2.3 (Deterministic Error Envelope Standardization):
//                  the one place every unhandled exception across every
//                  controller (Facility/Employee/Machine/WorkflowEngine/
//                  AI.*/Enterprise.*) is converted into a standardized
//                  ErrorEnvelope JSON response. Wired in Program.cs via
//                  app.UseExceptionHandler(...). Reads/echoes the
//                  X-Correlation-Id header PlantSafetyApiClient already
//                  sends on every request (Services/PlantSafetyApiClient.cs),
//                  so a single correlation id ties a UI action to its
//                  server-side error log entry. Phase 2.4: logs through
//                  ILogEventWriter (structured, JSON-formattable) instead
//                  of a raw ILogger message template.
// ============================================================================
using Microsoft.AspNetCore.Diagnostics;

using Shared.Deterministic.Abstractions.Clock;
using Shared.ErrorHandling.Abstractions.Envelope;
using Shared.ErrorHandling.Abstractions.Policy;
using Shared.Telemetry.Abstractions.Events;

namespace PlantSafety.UI.Api.Middleware;

/// <summary>Converts unhandled exceptions into standardized ErrorEnvelope JSON responses.</summary>
public static class GlobalErrorHandler
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    /// <summary>Handles the current unhandled exception and writes the standardized JSON response.</summary>
    public static async Task HandleAsync(HttpContext context, IClock clock, IErrorPolicy errorPolicy, CircuitBreaker circuitBreaker, ILogEventWriter logEventWriter)
    {
        IExceptionHandlerPathFeature? feature = context.Features.Get<IExceptionHandlerPathFeature>();
        Exception exception = feature?.Error ?? new Exception("Unknown error.");
        string module = ModuleFromPath(feature?.Path ?? context.Request.Path.Value ?? string.Empty);
        string correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeader, out var headerValue) && !string.IsNullOrWhiteSpace(headerValue)
            ? headerValue.ToString()
            : Guid.NewGuid().ToString();

        ErrorEnvelope envelope = ErrorEnvelopeFactory.FromException(exception, module, correlationId, clock.UtcNow);
        ErrorPolicyDecision decision = errorPolicy.Evaluate(envelope, attemptNumber: 1);

        if (decision.ShouldEscalate)
        {
            circuitBreaker.RecordFailure();
        }

        logEventWriter.Write(new LogEvent(
            EventId: envelope.ErrorCode,
            Category: LogCategoryClassifier.Classify(module),
            Severity: LogSeverity.Error,
            Message: envelope.DeveloperMessage,
            Payload: new Dictionary<string, object?>
            {
                ["ErrorCategory"] = envelope.Category.ToString(),
                ["PolicyDecision"] = decision.Reason,
                ["ShouldRetry"] = decision.ShouldRetry,
                ["ShouldEscalate"] = decision.ShouldEscalate,
            },
            CorrelationId: correlationId,
            Module: module,
            OccurredAtUtc: envelope.OccurredAtUtc));

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = ErrorCategoryHttpMapping.ToHttpStatusCode(envelope.Category);
        if (decision.ShouldRetry)
        {
            context.Response.Headers.RetryAfter = ((int)Math.Ceiling(decision.RetryDelay.TotalSeconds)).ToString();
        }

        await context.Response.WriteAsJsonAsync(envelope);
    }

    private static string ModuleFromPath(string path)
    {
        string[] segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        int uiIndex = Array.IndexOf(segments, "ui");
        if (uiIndex < 0 || uiIndex + 1 >= segments.Length)
        {
            return segments.Length > 0 ? segments[^1] : "PlantSafety.UI.Api";
        }

        // "api/v1/ui/engines/<engine-name>/..." -- the meaningful module is the engine
        // name, not the generic "engines" grouping segment.
        return segments[uiIndex + 1] == "engines" && uiIndex + 2 < segments.Length
            ? segments[uiIndex + 2]
            : segments[uiIndex + 1];
    }
}
