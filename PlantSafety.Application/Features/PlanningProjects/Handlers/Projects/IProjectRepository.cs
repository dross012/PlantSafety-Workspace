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
        string name,
        string description,
        string priority,
        Guid projectOwnerId,
        Guid? projectManagerId,
        DateTimeOffset plannedStartUtc,
        DateTimeOffset plannedEndUtc,
        decimal? plannedBudget,
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
    /// <summary>Project name.</summary>
    public required string Name { get; init; }
    /// <summary>Description.</summary>
    public required string Description { get; init; }
    /// <summary>Lifecycle status.</summary>
    public required string Status { get; init; }
    /// <summary>Priority.</summary>
    public required string Priority { get; init; }
    /// <summary>Project owner/sponsor identifier.</summary>
    public required Guid ProjectOwnerId { get; init; }
    /// <summary>Project manager identifier.</summary>
    public Guid? ProjectManagerId { get; init; }
    /// <summary>Planned start timestamp.</summary>
    public required DateTimeOffset PlannedStartUtc { get; init; }
    /// <summary>Planned end timestamp.</summary>
    public required DateTimeOffset PlannedEndUtc { get; init; }
    /// <summary>Actual start timestamp.</summary>
    public DateTimeOffset? ActualStartUtc { get; init; }
    /// <summary>Actual end timestamp.</summary>
    public DateTimeOffset? ActualEndUtc { get; init; }
    /// <summary>Planned budget.</summary>
    public decimal? PlannedBudget { get; init; }
    /// <summary>Open milestone count.</summary>
    public required int OpenMilestoneCount { get; init; }
    /// <summary>Last modification timestamp.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }
    /// <summary>Row version.</summary>
    public required string ETag { get; init; }
}
