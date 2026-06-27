// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Query to fetch a single Safety Project by identifier.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Behaviors.Caching;
using PlantSafety.Application.Features.PlanningProjects.Models.Projects;

namespace PlantSafety.Application.Features.PlanningProjects.Queries.Projects.GetSafetyProject;

/// <summary>Returns the <see cref="SafetyProjectDto"/> for a given project, or null.</summary>
public sealed record GetSafetyProjectByIdQuery : IQuery<SafetyProjectDto?>, ICacheableQuery
{
    /// <inheritdoc/>
    public required Guid TraceId { get; init; }

    /// <inheritdoc/>
    public required Guid SpanId { get; init; }

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

    /// <summary>Project to retrieve.</summary>
    public required Guid ProjectId { get; init; }

    /// <inheritdoc/>
    public string CacheKey => $"projects:{TenantId}:{ProjectId}";

    /// <inheritdoc/>
    public TimeSpan CacheExpiry => TimeSpan.FromMinutes(5);
}
