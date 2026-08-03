// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// LAST UPDATED: 2026-08-03 (Claude Code performance-optimization-agent) -- Regression audit fix:
//   this behavior runs FIRST for every command/query in the platform, yet it previously emitted
//   message-template logs ("Handling {RequestType} | ...") instead of the mandated single
//   structured envelope object (Handbook §2.19 Success Logging Architectural Rule, §2.22
//   Canonical Log Envelope Rule -- "no message templates", "no concatenated strings"), had no
//   SchemaVersion field (§2.33), and resolved IApplicationLogger nowhere (that abstraction had
//   zero consumers anywhere in the solution). Also emitted a "Handling" pre-log for every single
//   request, which is unnecessary volume per §2.27 (Anti-Noise Rule) once a single terminal
//   success/error envelope already carries full lineage -- removed in favor of the canonical
//   single-envelope handler shape from §2.30 Example C.
// FILE DESCRIPTION: MediatR pipeline behavior that emits SIEM-safe structured logs for every request.
//   Executes FIRST in the pipeline: Logging → Performance → Validation → Transaction → Handler.
//   Emits a success envelope on completion and an error envelope on exception.
//   Never logs PII, secrets, SQL text, or full stack traces. Uses StackTraceHash only.
// SchemaVersion: 2026.1
// Additive-only evolution: new fields may be added; no behavior may be altered destructively.

using System.Diagnostics;
using MediatR;
using PlantSafety.Application.Abstractions.Context;
using PlantSafety.Application.Abstractions.CurrentUser;
using PlantSafety.Application.Abstractions.Logging;
using PlantSafety.Application.Common.Logging;

namespace PlantSafety.Application.Behaviors.Logging;

/// <summary>
/// MediatR pipeline behavior that emits a canonical SIEM-safe structured log envelope
/// for every command and query passing through the pipeline.
/// Position in pipeline: first (before Performance, Validation, Transaction, Handler).
/// </summary>
/// <typeparam name="TRequest">The MediatR request type (ICommand or IQuery).</typeparam>
/// <typeparam name="TResponse">The response type produced by the handler.</typeparam>
/// <remarks>
/// Initializes a new instance of <see cref="LoggingBehavior{TRequest,TResponse}"/>.
/// </remarks>
/// <param name="applicationLogger">The canonical structured-envelope logger.</param>
/// <param name="operationContext">The current operation context carrying correlation lineage.</param>
/// <param name="currentUserService">The current actor identity, for envelope ActorId/TenantId.</param>
public sealed class LoggingBehavior<TRequest, TResponse>(
    IApplicationLogger applicationLogger,
    IOperationContext operationContext,
    ICurrentUserService currentUserService) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Executes the next behavior in the pipeline and emits exactly one structured log envelope
    /// (success or error) once the operation completes, per the Canonical Log Envelope Rule (§2.22).
    /// </summary>
    /// <param name="request">The incoming MediatR request.</param>
    /// <param name="next">The delegate to the next behavior or final handler.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    /// <exception cref="Exception">Re-throws any exception from downstream after logging.</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next(cancellationToken);
            stopwatch.Stop();

            applicationLogger.LogSuccess(new OperationLogEnvelope
            {
                OperationName = requestType,
                HandlerType = requestType,
                CorrelationId = operationContext.CorrelationId,
                CausationId = operationContext.CausationId,
                RequestId = operationContext.RequestId,
                TraceId = operationContext.TraceId,
                SpanId = operationContext.SpanId,
                TenantId = operationContext.TenantId,
                ActorId = currentUserService.IsAuthenticated ? currentUserService.ActorId : null,
                TimestampUtc = DateTime.UtcNow,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Success = true,
                Severity = LogSeverity.Information
            });

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var sanitized = ExceptionSanitizer.Sanitize(ex);

            applicationLogger.LogError(new OperationLogEnvelope
            {
                OperationName = requestType,
                HandlerType = requestType,
                CorrelationId = operationContext.CorrelationId,
                CausationId = operationContext.CausationId,
                RequestId = operationContext.RequestId,
                TraceId = operationContext.TraceId,
                SpanId = operationContext.SpanId,
                TenantId = operationContext.TenantId,
                ActorId = currentUserService.IsAuthenticated ? currentUserService.ActorId : null,
                TimestampUtc = DateTime.UtcNow,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Success = false,
                ExceptionType = sanitized.ExceptionType,
                Message = sanitized.Message,
                HResult = sanitized.HResult,
                StackTraceHash = sanitized.StackTraceHash,
                Severity = sanitized.Severity == "Warning" ? LogSeverity.Warning : LogSeverity.Error
            });

            throw;
        }
    }
}
