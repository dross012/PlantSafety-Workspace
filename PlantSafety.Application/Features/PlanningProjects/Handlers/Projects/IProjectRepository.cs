// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository interface for the SafetyProject aggregate.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.PlanningProjects.Handlers.Projects;

/// <summary>Repository for SafetyProject read/write operations.</summary>
public interface IProjectRepository
{
    /// <summary>Creates a new SafetyProject in Draft status; stages a <c>SafetyProjectCreatedEvent</c>.</summary>
    Task CreateAsync(
        Guid projectId,
        Guid tenantId,
        Guid siteId,
        string title,
        string description,
        string projectType,
        DateTimeOffset plannedStartUtc,
        DateTimeOffset? plannedEndUtc,
        Guid ownerId,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Returns the read model, or null if not found.</summary>
    Task<ProjectReadModel?> GetByIdAsync(Guid tenantId, Guid projectId, CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for SafetyProject stored-procedure result sets.</summary>
public sealed record ProjectReadModel
{
    /// <summary>Project identifier.</summary>
    public required Guid ProjectId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Site identifier.</summary>
    public required Guid SiteId { get; init; }
    /// <summary>Title.</summary>
    public required string Title { get; init; }
    /// <summary>Description.</summary>
    public required string Description { get; init; }
    /// <summary>Project type.</summary>
    public required string ProjectType { get; init; }
    /// <summary>Lifecycle status.</summary>
    public required string Status { get; init; }
    /// <summary>Planned start timestamp.</summary>
    public required DateTimeOffset PlannedStartUtc { get; init; }
    /// <summary>Planned end timestamp.</summary>
    public required DateTimeOffset? PlannedEndUtc { get; init; }
    /// <summary>Owner identifier.</summary>
    public required Guid OwnerId { get; init; }
    /// <summary>Open milestone count.</summary>
    public required int OpenMilestoneCount { get; init; }
    /// <summary>Last modification timestamp.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }
    /// <summary>Row version.</summary>
    public required string ETag { get; init; }
}
