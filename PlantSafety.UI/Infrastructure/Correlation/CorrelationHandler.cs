// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Delegating HTTP message handler that injects the full
//   correlation lineage envelope into every outbound API request. Applied
//   to all typed HttpClient registrations via DI pipeline ordering.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §2.17
// ============================================================

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using PlantSafety.UI.Infrastructure.Correlation;

namespace PlantSafety.UI.Infrastructure.Correlation;

/// <summary>
/// Delegating message handler that stamps every outbound HTTP request with the
/// current correlation lineage headers so the API layer can reconstruct the
/// full distributed trace without any component-level boilerplate.
/// </summary>
/// <remarks>
/// Header contract (must match API middleware expectations):
///   X-Correlation-Id   → CorrelationContext.CorrelationId
///   X-Causation-Id     → CorrelationContext.CausationId (omitted when null)
///   X-Request-Id       → CorrelationContext.RequestId
///   X-Tenant-Id        → CorrelationContext.TenantId
/// </remarks>
public sealed class CorrelationHandler : DelegatingHandler
{
    private readonly CorrelationContext _correlation;

    /// <summary>
    /// Initializes a new instance of <see cref="CorrelationHandler"/>.
    /// </summary>
    /// <param name="correlation">The scoped correlation context for the active session.</param>
    public CorrelationHandler(CorrelationContext correlation)
    {
        _correlation = correlation;
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.TryAddWithoutValidation("X-Correlation-Id", _correlation.CorrelationId.ToString());
        request.Headers.TryAddWithoutValidation("X-Request-Id", _correlation.RequestId.ToString());

        if (_correlation.CausationId.HasValue)
        {
            request.Headers.TryAddWithoutValidation("X-Causation-Id", _correlation.CausationId.Value.ToString());
        }

        if (_correlation.TenantId != default)
        {
            request.Headers.TryAddWithoutValidation("X-Tenant-Id", _correlation.TenantId.ToString());
        }

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.Headers.TryGetValues("X-Trace-Id", out var traceValues) &&
            response.Headers.TryGetValues("X-Span-Id", out var spanValues))
        {
            var traceId = string.Join(string.Empty, traceValues);
            var spanId = string.Join(string.Empty, spanValues);
            _correlation.ApplyBackendTrace(traceId, spanId);
        }

        return response;
    }
}
