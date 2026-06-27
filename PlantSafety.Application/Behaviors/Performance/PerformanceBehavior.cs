// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: MediatR pipeline behavior that measures request execution time and emits
//   a structured warning when requests exceed the configured SLA threshold.
//   Executes SECOND in the pipeline: Logging → Performance → Validation → Transaction → Handler.
//   Default threshold: 500ms for commands, 200ms for queries.
// SchemaVersion: 1
// Additive-only evolution: threshold and fields may be added; no behavior may be removed destructively.

using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlantSafety.Application.Abstractions.Context;
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
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly IOperationContext _operationContext;
    private readonly PerformanceOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="PerformanceBehavior{TRequest,TResponse}"/>.
    /// </summary>
    /// <param name="logger">The structured logger.</param>
    /// <param name="operationContext">The current operation context carrying correlation lineage.</param>
    /// <param name="options">Configuration options for the performance behavior.</param>
    public PerformanceBehavior(
        ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
        IOperationContext operationContext,
        IOptions<PerformanceOptions> options)
    {
        _logger = logger;
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
            _logger.LogWarning(
                "SlowRequest | RequestType={RequestType} | ElapsedMs={ElapsedMs} | ThresholdMs={ThresholdMs} | CorrelationId={CorrelationId} | TraceId={TraceId}",
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds,
                _options.SlowRequestThresholdMs,
                _operationContext.CorrelationId,
                _operationContext.TraceId);
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
