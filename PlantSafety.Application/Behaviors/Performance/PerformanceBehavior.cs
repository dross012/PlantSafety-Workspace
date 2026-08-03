// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// LAST UPDATED: 2026-08-03 (Claude Code performance-optimization-agent) -- Regression audit fix
//   (Handbook §2.19-2.22, Performance & Stress Manual): the slow-request warning previously used
//   a message template ("SlowRequest | RequestType=... | ...") instead of a single structured
//   envelope object, and had no SchemaVersion field. Converted to emit via IApplicationLogger.
// FILE DESCRIPTION: MediatR pipeline behavior that measures request execution time and emits
//   a structured warning when requests exceed the configured SLA threshold.
//   Executes SECOND in the pipeline: Logging → Performance → Validation → Transaction → Handler.
//   Default threshold: 500ms for commands, 200ms for queries.
// SchemaVersion: 2026.1
// Additive-only evolution: threshold and fields may be added; no behavior may be removed destructively.

using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Options;
using PlantSafety.Application.Abstractions.Context;
using PlantSafety.Application.Abstractions.Logging;
using PlantSafety.Application.Common.Constants;

namespace PlantSafety.Application.Behaviors.Performance;

/// <summary>
/// MediatR pipeline behavior that tracks request execution duration.
/// Emits a structured warning log when execution exceeds <see cref="PerformanceOptions.SlowRequestThresholdMs"/>.
/// Position in pipeline: second (after Logging, before Validation, Transaction, and Handler).
/// </summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The response type produced by the handler.</typeparam>
public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IApplicationLogger _applicationLogger;
    private readonly IOperationContext _operationContext;
    private readonly PerformanceOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="PerformanceBehavior{TRequest,TResponse}"/>.
    /// </summary>
    /// <param name="applicationLogger">The canonical structured-envelope logger.</param>
    /// <param name="operationContext">The current operation context carrying correlation lineage.</param>
    /// <param name="options">Configuration options for the performance behavior.</param>
    public PerformanceBehavior(
        IApplicationLogger applicationLogger,
        IOperationContext operationContext,
        IOptions<PerformanceOptions> options)
    {
        _applicationLogger = applicationLogger;
        _operationContext = operationContext;
        _options = options.Value;
    }

    /// <summary>
    /// Times the downstream pipeline execution and emits a slow-request warning if the threshold is exceeded.
    /// </summary>
    /// <param name="request">The incoming MediatR request.</param>
    /// <param name="next">The delegate to the next behavior or final handler.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > _options.SlowRequestThresholdMs)
        {
            _applicationLogger.LogWarning(new OperationLogEnvelope
            {
                OperationName = typeof(TRequest).Name,
                HandlerType = typeof(TRequest).Name,
                CorrelationId = _operationContext.CorrelationId,
                CausationId = _operationContext.CausationId,
                RequestId = _operationContext.RequestId,
                TraceId = _operationContext.TraceId,
                SpanId = _operationContext.SpanId,
                TenantId = _operationContext.TenantId,
                TimestampUtc = DateTime.UtcNow,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Success = true,
                Severity = LogSeverity.Warning
            });
        }

        return response;
    }
}

/// <summary>
/// Configuration options for the <see cref="PerformanceBehavior{TRequest,TResponse}"/>.
/// </summary>
public sealed class PerformanceOptions
{
    /// <summary>Gets or sets the elapsed millisecond threshold above which a request is classified as slow. Default: 500ms.</summary>
    public long SlowRequestThresholdMs { get; set; } = ApplicationConstants.DefaultSlowRequestThresholdMs;
}
