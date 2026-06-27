// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: MediatR pipeline behavior that emits Prometheus metrics for every request.
//   Records: request count, error count, duration histogram.
//   Metric naming follows the canonical rule: module.operation.metric format.
//   No high-cardinality labels (no per-request IDs in label sets).
// SchemaVersion: 1
// Additive-only evolution: new metric dimensions may be added; no existing metrics may be removed.

using System.Diagnostics;
using System.Diagnostics.Metrics;
using MediatR;

namespace PlantSafety.Application.Behaviors.Metrics;

/// <summary>
/// MediatR pipeline behavior that records request count, error count, and duration histogram
/// using .NET Meter API (compatible with Prometheus / OpenTelemetry export).
/// No high-cardinality labels are used; only request type and outcome.
/// </summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The response type produced by the handler.</typeparam>
public sealed class MetricsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly Meter _meter = new("PlantSafety.Application", "1.0");

    private static readonly Counter<long> _requestCounter = _meter.CreateCounter<long>(
        "plantsafety.application.requests.total",
        description: "Total number of Application layer requests processed.");

    private static readonly Counter<long> _errorCounter = _meter.CreateCounter<long>(
        "plantsafety.application.requests.errors.total",
        description: "Total number of Application layer request failures.");

    private static readonly Histogram<double> _durationHistogram = _meter.CreateHistogram<double>(
        "plantsafety.application.requests.duration_ms",
        unit: "ms",
        description: "Duration of Application layer request processing in milliseconds.");

    /// <summary>
    /// Records request metrics for the current pipeline execution.
    /// </summary>
    /// <param name="request">The incoming MediatR request.</param>
    /// <param name="next">The delegate to the next behavior or final handler.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        var tags = new TagList { { "request_type", requestType } };

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await next(cancellationToken);
            stopwatch.Stop();

            _requestCounter.Add(1, new TagList { { "request_type", requestType }, { "success", "true" } });
            _durationHistogram.Record(stopwatch.ElapsedMilliseconds, tags);

            return response;
        }
        catch
        {
            stopwatch.Stop();
            _errorCounter.Add(1, new TagList { { "request_type", requestType } });
            _durationHistogram.Record(stopwatch.ElapsedMilliseconds, tags);
            throw;
        }
    }
}
