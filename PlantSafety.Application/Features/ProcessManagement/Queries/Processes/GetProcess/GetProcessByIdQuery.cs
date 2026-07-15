// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Query to fetch a single Process Definition by identifier.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Caching;
using PlantSafety.Application.Features.ProcessManagement.Models.Processes;

namespace PlantSafety.Application.Features.ProcessManagement.Queries.Processes.GetProcess;

/// <summary>Returns the <see cref="ProcessDefinitionDto"/> for a given process, or null.</summary>
public sealed record GetProcessByIdQuery : IQuery<ProcessDefinitionDto?>, ICacheableQuery
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
    public CachePreference CachePreference { get; init; } = CachePreference.Preferred;

    /// <inheritdoc/>
    public bool SuppressUnauthorized { get; init; }

    /// <inheritdoc/>
    public bool StrictMode { get; init; }

    /// <summary>Process to retrieve.</summary>
    public required Guid ProcessId { get; init; }

    /// <inheritdoc/>
    public string CacheKey => $"processes:{TenantId}:{ProcessId}";

    /// <inheritdoc/>
    public TimeSpan CacheExpiry => TimeSpan.FromMinutes(10);
}
