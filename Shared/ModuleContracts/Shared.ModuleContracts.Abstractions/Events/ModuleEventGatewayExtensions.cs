// ============================================================================
//  Project:        Shared.ModuleContracts
//  Module:         Abstractions.Events
//  File:           ModuleEventGatewayExtensions.cs
//  Created:        2026-08-09
//  Description:    Phase 3.4 deliverable: the "isolated publish" policy.
//                  ModuleEventGateway.PublishAsync deliberately rethrows a
//                  subscriber's failure (see its own header comment) so a
//                  caller that genuinely needs to know a subscriber failed
//                  still can. But most publishers -- a controller that just
//                  finished a real write and is telling other modules "this
//                  happened" -- should not have an unrelated subscriber's bug
//                  turn an already-successful write into a 500 response to
//                  the client. PublishIsolatedAsync is the seam for that:
//                  publish, and if it fails, log the failure and swallow it
//                  rather than letting it propagate into the caller's
//                  success path.
// ============================================================================
using Shared.Telemetry.Abstractions.Events;

namespace Shared.ModuleContracts.Abstractions.Events;

/// <summary>Extension methods for IModuleEventGateway.</summary>
public static class ModuleEventGatewayExtensions
{
    /// <summary>Publishes the event; if any subscriber fails, logs the failure via ILogEventWriter and does not rethrow.</summary>
    public static async Task PublishIsolatedAsync<TEvent>(
        this IModuleEventGateway gateway,
        TEvent moduleEvent,
        ILogEventWriter logEventWriter,
        string correlationId,
        CancellationToken cancellationToken = default) where TEvent : IModuleEvent
    {
        try
        {
            await gateway.PublishAsync(moduleEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            logEventWriter.Write(new LogEvent(
                EventId: "MODULE_EVENT_PUBLISH_FAILED",
                Category: LogCategory.Infrastructure,
                Severity: LogSeverity.Error,
                Message: $"One or more subscribers to {typeof(TEvent).Name} failed: {ex.Message}",
                Payload: new Dictionary<string, object?>
                {
                    ["EventType"] = typeof(TEvent).Name,
                    ["ExceptionType"] = ex.GetType().Name,
                },
                CorrelationId: correlationId,
                Module: moduleEvent.SourceModule,
                OccurredAtUtc: moduleEvent.OccurredAtUtc));
        }
    }
}
