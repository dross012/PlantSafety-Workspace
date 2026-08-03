// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for ListUsersQuery. Maps IUserRepository read models onto the
//   public UserProfileDto shape.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.EnterpriseFabric.Models.Users;
using PlantSafety.Application.Features.EnterpriseFabric.Queries.Users.List;

namespace PlantSafety.Application.Features.EnterpriseFabric.Handlers.Users;

/// <summary>Handles <see cref="ListUsersQuery"/>.</summary>
public sealed class ListUsersQueryHandler : IQueryHandler<ListUsersQuery, PagedUsersResult>
{
    private readonly IUserRepository _repository;
    private readonly ILogger<ListUsersQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListUsersQueryHandler(IUserRepository repository, ILogger<ListUsersQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PagedUsersResult> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Listing users for Tenant {TenantId}, page {PageNumber}", request.TenantId, request.PageNumber);

        var (items, totalCount) = await _repository.ListAsync(
            request.TenantId,
            request.Role,
            request.SiteId,
            request.IsActive,
            request.OrderBy,
            request.SortDirection,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return new PagedUsersResult
        {
            Items = items.Select(MapProfile).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
        };
    }

    private static UserProfileDto MapProfile(UserReadModel model) => new()
    {
        UserId = model.UserId,
        TenantId = model.TenantId,
        DisplayName = $"{model.FirstName} {model.LastName}".Trim(),
        Email = model.Email,
        Roles = model.Roles,
        SiteIds = model.SiteIds,
        Status = model.IsActive ? "Active" : "Deactivated",
        LastLoginAtUtc = null,
        CreatedAtUtc = model.LastModifiedAtUtc,
        LastModifiedAtUtc = model.LastModifiedAtUtc,
    };
}
