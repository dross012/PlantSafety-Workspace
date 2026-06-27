// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: UI-side correlation context carrier. Holds the full
//   lineage envelope (CorrelationId, CausationId, RequestId, TraceId,
//   SpanId) for the active UI session. Injected as a scoped service so
//   every component and HTTP handler participates in the same lineage.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §2.17
// ============================================================

using System;

namespace PlantSafety.UI.Infrastructure.Correlation;

/// <summary>
/// Carries the full distributed-tracing lineage envelope for a single UI operation.
/// Scoped per Blazor render tree; reset on each top-level navigation.
/// </summary>
/// <remarks>
/// All HTTP handlers and state containers MUST read from this context rather than
/// generating their own identifiers to guarantee lineage continuity across layers.
/// </remarks>
public sealed class CorrelationContext
{
    /// <summary>Gets or sets the correlation identifier that groups all operations in a logical unit.</summary>
    public Guid CorrelationId { get; private set; } = Guid.NewGuid();

    /// <summary>Gets or sets the causation identifier inherited from the triggering event or command.</summary>
    public Guid? CausationId { get; private set; }

    /// <summary>Gets or sets the request identifier for this specific inbound HTTP call.</summary>
    public Guid RequestId { get; private set; } = Guid.NewGuid();

    /// <summary>Gets or sets the distributed trace identifier propagated from the API gateway.</summary>
    public string TraceId { get; private set; } = string.Empty;

    /// <summary>Gets or sets the current execution span identifier.</summary>
    public string SpanId { get; private set; } = string.Empty;

    /// <summary>Gets or sets the tenant identifier for the active authenticated session.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Gets or sets the user identifier for the active authenticated session.</summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Resets the correlation envelope for a new top-level operation. Generates a new
    /// <see cref="CorrelationId"/> and <see cref="RequestId"/>; clears causation lineage.
    /// </summary>
    public void BeginOperation()
    {
        CorrelationId = Guid.NewGuid();
        RequestId = Guid.NewGuid();
        CausationId = null;
    }

    /// <summary>
    /// Sets the causation identifier when the current operation was triggered by a parent
    /// command or event. The causation ID is inherited, never regenerated.
    /// </summary>
    /// <param name="causationId">The identifier of the triggering command or event.</param>
    public void SetCausation(Guid causationId) => CausationId = causationId;

    /// <summary>
    /// Applies the trace context received from an API response header, ensuring the
    /// UI-side context remains synchronized with the backend trace.
    /// </summary>
    /// <param name="traceId">The W3C trace-id value from the traceparent header.</param>
    /// <param name="spanId">The W3C span-id value from the traceparent header.</param>
    public void ApplyBackendTrace(string traceId, string spanId)
    {
        TraceId = traceId ?? string.Empty;
        SpanId = spanId ?? string.Empty;
    }

    /// <summary>
    /// Binds the session identity to this correlation context so all downstream
    /// log entries carry tenant and user context.
    /// </summary>
    /// <param name="tenantId">The authenticated tenant identifier.</param>
    /// <param name="userId">The authenticated user identifier.</param>
    public void BindIdentity(Guid tenantId, Guid userId)
    {
        TenantId = tenantId;
        UserId = userId;
    }

    /// <summary>
    /// Returns a SIEM-safe structured envelope suitable for inclusion in log payloads.
    /// Never includes PII, secrets, or stack traces.
    /// </summary>
    /// <returns>An anonymous object with all lineage fields for structured logging.</returns>
    public object ToLogEnvelope() => new
    {
        CorrelationId,
        CausationId,
        RequestId,
        TraceId,
        SpanId,
        TenantId,
        UserId
    };
}
