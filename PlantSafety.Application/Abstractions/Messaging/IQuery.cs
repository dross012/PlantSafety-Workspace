// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Canonical marker interface for Queries in the CQRS pipeline.
//   Queries MUST NEVER mutate state. They use read-only Dapper calls exclusively.
//   Required envelope fields are mandated by §2.2 of the Enterprise Architectural Rule Handbook.
// SchemaVersion: 1
// Additive-only evolution: new members may be added; no fields may be removed or renamed.

using MediatR;

namespace PlantSafety.Application.Abstractions.Messaging;

/// <summary>
/// Marker interface for queries that return a typed result.
/// Queries are read-only; they must never mutate state, never trigger events,
/// and never participate in transactions.
/// </summary>
/// <typeparam name="TResponse">The strongly typed read model returned by the query handler.</typeparam>
public interface IQuery<TResponse> : IRequest<TResponse>
{
    /// <summary>Gets the distributed trace identifier for OpenTelemetry propagation.</summary>
    string TraceId { get; }

    /// <summary>Gets the current span identifier for OpenTelemetry.</summary>
    string SpanId { get; }

    /// <summary>Gets the API version token for versioned query evolution.</summary>
    string ApiVersionToken { get; }

    /// <summary>Gets the identifier of the user executing the query.</summary>
    Guid RequestingUserId { get; }

    /// <summary>Gets the role of the requesting user for authorization decisions.</summary>
    string RequestingUserRole { get; }

    /// <summary>Gets the tenant identifier for multi-tenancy data isolation.</summary>
    Guid TenantId { get; }

    /// <summary>Gets the correlation identifier for lineage tracking.</summary>
    Guid CorrelationId { get; }

    /// <summary>Gets the cache preference for this query (None, Preferred, Required).</summary>
    CachePreference CachePreference { get; }

    /// <summary>Gets whether to suppress unauthorized items rather than throw an authorization exception.</summary>
    bool SuppressUnauthorized { get; }

    /// <summary>Gets whether to apply strict validation mode (reject partial or ambiguous inputs).</summary>
    bool StrictMode { get; }
}

/// <summary>
/// Specifies the caller's cache preference for a query result.
/// </summary>
public enum CachePreference
{
    /// <summary>Do not use cache; always execute the query.</summary>
    None = 0,

    /// <summary>Use cache if available; fall through to storage on miss.</summary>
    Preferred = 1,

    /// <summary>Require a cache hit; throw if cache is unavailable.</summary>
    Required = 2
}
