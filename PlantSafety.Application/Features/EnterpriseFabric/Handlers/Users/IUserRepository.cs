// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository interface for the User aggregate.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.EnterpriseFabric.Handlers.Users;

/// <summary>Repository for User read/write operations.</summary>
public interface IUserRepository
{
    /// <summary>Creates a new User; stages a <c>UserCreatedEvent</c> in the outbox.</summary>
    Task CreateAsync(
        Guid userId,
        Guid tenantId,
        string email,
        string firstName,
        string lastName,
        IReadOnlyList<string> roles,
        IReadOnlyList<Guid> siteIds,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken);

    /// <summary>Returns the read model, or null if not found.</summary>
    Task<UserReadModel?> GetByIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken);

    /// <summary>Returns paged records matching the given filters.</summary>
    Task<(IReadOnlyList<UserReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        string? role,
        Guid? siteId,
        bool? isActive,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for User stored-procedure result sets.</summary>
public sealed record UserReadModel
{
    /// <summary>User identifier.</summary>
    public required Guid UserId { get; init; }
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Email address (internal use; never logged).</summary>
    public required string Email { get; init; }
    /// <summary>First name.</summary>
    public required string FirstName { get; init; }
    /// <summary>Last name.</summary>
    public required string LastName { get; init; }
    /// <summary>Assigned roles.</summary>
    public required IReadOnlyList<string> Roles { get; init; }
    /// <summary>Assigned site identifiers.</summary>
    public required IReadOnlyList<Guid> SiteIds { get; init; }
    /// <summary>Account active flag.</summary>
    public required bool IsActive { get; init; }
    /// <summary>Last modification timestamp.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }
    /// <summary>Row version.</summary>
    public required string ETag { get; init; }
}
