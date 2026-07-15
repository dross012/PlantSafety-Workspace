// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Query to list integration fabric nodes for a tenant.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.EnterpriseFabric.Models.FabricNodes;

namespace PlantSafety.Application.Features.EnterpriseFabric.Queries.FabricNodes.ListNodes;

/// <summary>Returns the integration fabric node registry for a tenant.</summary>
public sealed record ListFabricNodesQuery : IQuery<IReadOnlyList<FabricNodeDto>>
{
    /// <inheritdoc/>
    public required string TraceId { get; init; }

    /// <inheritdoc/>
    public required string SpanId { get; init; }

    /// <inheritdoc/>
    public required string ApiVersionToken { get; init; }

    /// <inheritdoc/>
    public required Guid RequestingUserId { get; init; }

    /// <inheritdoc/>
    public required string RequestingUserRole { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public CachePreference CachePreference { get; init; } = CachePreference.None;

    /// <inheritdoc/>
    public bool SuppressUnauthorized { get; init; }

    /// <inheritdoc/>
    public bool StrictMode { get; init; }

    /// <summary>Optional site filter.</summary>
    public Guid? SiteId { get; init; }
}
