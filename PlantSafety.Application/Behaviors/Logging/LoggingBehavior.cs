// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: MediatR pipeline behavior that emits SIEM-safe structured logs for every request.
//   Executes FIRST in the pipeline: Logging → Performance → Validation → Transaction → Handler.
//   Emits a success envelope on completion and an error envelope on exception.
//   Never logs PII, secrets, SQL text, or full stack traces. Uses StackTraceHash only.
// SchemaVersion: 1
// Additive-only evolution: new fields may be added; no behavior may be altered destructively.

using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Context;

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
/// <param name="logger">The structured logger.</param>
/// <param name="operationContext">The current operation context carrying correlation lineage.</param>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    IOperationContext operationContext) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;
    private readonly IOperationContext _operationContext = operationContext;

    /// <summary>
    /// Executes the next behavior in the pipeline and emits structured log envelopes before and after.
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

        _logger.LogInformation(
            "Handling {RequestType} | CorrelationId={CorrelationId} | CausationId={CausationId} | TraceId={TraceId}",
            requestType,
            _operationContext.CorrelationId,
            _operationContext.CausationId,
            _operationContext.TraceId);

        try
        {
            var response = await next(cancellationToken);
            stopwatch.Stop();

            _logger.LogInformation(
                "Handled {RequestType} | Success=true | ElapsedMs={ElapsedMs} | CorrelationId={CorrelationId} | TraceId={TraceId}",
                requestType,
                stopwatch.ElapsedMilliseconds,
                _operationContext.CorrelationId,
                _operationContext.TraceId);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Failed {RequestType} | Success=false | ElapsedMs={ElapsedMs} | CorrelationId={CorrelationId} | ExceptionType={ExceptionType} | HResult={HResult} | StackTraceHash={StackTraceHash}",
                requestType,
                stopwatch.ElapsedMilliseconds,
                _operationContext.CorrelationId,
                ex.GetType().Name,
                ex.HResult,
                ComputeStackTraceHash(ex));

            throw;
        }
    }

    private static string ComputeStackTraceHash(Exception ex)
    {
        var raw = ex.StackTrace ?? string.Empty;
        var hash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash)[..16];
    }
}
