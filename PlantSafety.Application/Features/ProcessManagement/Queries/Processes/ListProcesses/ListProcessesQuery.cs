// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Query to list Process Definitions for a tenant.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.ProcessManagement.Models.Processes;

namespace PlantSafety.Application.Features.ProcessManagement.Queries.Processes.ListProcesses;

/// <summary>Returns paginated <see cref="ProcessDefinitionDto"/> records.</summary>
public sealed record ListProcessesQuery : IQuery<PaginatedResult<ProcessDefinitionDto>>
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

    /// <summary>1-based page number.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Items per page.</summary>
    public int PageSize { get; init; } = 25;

    /// <summary>Sort field (ProcessName, Category, Status).</summary>
    public string OrderBy { get; init; } = "ProcessName";

    /// <summary>Sort direction.</summary>
    public string SortDirection { get; init; } = "ASC";

    /// <summary>Optional site filter.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional category filter.</summary>
    public string? Category { get; init; }

    /// <summary>Optional status filter.</summary>
    public string? Status { get; init; }
}
