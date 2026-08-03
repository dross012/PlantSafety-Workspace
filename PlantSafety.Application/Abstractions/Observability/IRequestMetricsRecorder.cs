// AUTHOR: PlantSafety Platform Team (Claude Code performance-optimization-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Presentation-layer request-metrics capture abstraction (Handbook Phase 6
//   §5.3 Metrics & Dashboards -- PlatformHealthDashboard). Records one sample per completed HTTP
//   request (method, path, status code, elapsed milliseconds) for later aggregation into
//   throughput/latency-percentile/error-rate KPIs. MUST be non-blocking and MUST NOT throw --
//   metrics capture is best-effort observability, never a request-path dependency.
// SchemaVersion: 2026.1
// Additive-only evolution: new members may be added; no members may be removed or renamed.

namespace PlantSafety.Application.Abstractions.Observability;

/// <summary>
/// Records HTTP request outcome/latency samples for the PlatformHealthDashboard read-model.
/// Implementations MUST be non-blocking (fire-and-forget, bounded, drop-on-overflow) so that
/// metrics capture never adds latency to, or faults, the request it is observing.
/// </summary>
public interface IRequestMetricsRecorder
{
    /// <summary>Records a single completed HTTP request sample. Never throws; never blocks the caller.</summary>
    /// <param name="method">The HTTP method (GET, POST, ...).</param>
    /// <param name="path">The request path.</param>
    /// <param name="statusCode">The HTTP response status code.</param>
    /// <param name="elapsedMs">Elapsed request duration in milliseconds.</param>
    /// <param name="correlationId">The request's correlation identifier.</param>
    /// <param name="tenantId">The request's tenant identifier, or <see cref="Guid.Empty"/> if unresolved.</param>
    void Record(string method, string path, int statusCode, long elapsedMs, Guid correlationId, Guid tenantId);
}
