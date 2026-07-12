// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.EnterpriseFabric.Users
//  Layer:          Infrastructure
//  File:           UserDapperRepository.cs
//  Created:        2026-07-12
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the User aggregate. All DB access via stored
//                  procedures in the [EnterpriseFabric] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.EnterpriseFabric.Handlers.Users;

namespace PlantSafety.Infrastructure.Features.EnterpriseFabric.Users.Repositories;

/// <summary>
/// Dapper-backed repository for the <c>User</c> aggregate.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class UserDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<UserDapperRepository> logger)
    : IUserRepository
{
    /// <inheritdoc/>
    public async Task CreateAsync(
        Guid userId,
        Guid tenantId,
        string email,
        string firstName,
        string lastName,
        IReadOnlyList<string> roles,
        IReadOnlyList<Guid> siteIds,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Create User {UserId} for tenant {TenantId}", userId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("UserId", userId)
            .AddString("Email", email, 320)
            .AddString("FirstName", firstName, 100)
            .AddString("LastName", lastName, 100)
            .AddString("Roles", string.Join(',', roles), 4000)
            .AddString("SiteIds", string.Join(',', siteIds), 4000)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[EnterpriseFabric].[usp_User_Create]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<UserReadModel?> GetByIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get User {UserId} for tenant {TenantId}", userId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("UserId", userId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<UserRow>(
            "[EnterpriseFabric].[usp_User_GetById]", parameters, cancellationToken);

        return row is null ? null : MapReadModel(row);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<UserReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        string? role,
        Guid? siteId,
        bool? isActive,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("List Users for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddString("Role", role, 100)
            .AddGuid("SiteId", siteId)
            .AddBool("IsActive", isActive)
            .AddString("OrderBy", orderBy, 100)
            .AddString("SortDirection", sortDirection, 4)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var rows = await queryExecutor.QueryAsync<UserListRow>(
            "[EnterpriseFabric].[usp_User_List]", parameters, cancellationToken);

        var items = rows.Select(MapReadModel).ToList();
        var totalCount = rows.Count > 0 ? rows[0].TotalCount : 0;
        return (items, totalCount);
    }

    private static UserReadModel MapReadModel(UserRow row) => new()
    {
        UserId = row.UserId,
        TenantId = row.TenantId,
        Email = row.Email,
        FirstName = row.FirstName,
        LastName = row.LastName,
        Roles = SplitCsv(row.RolesCsv),
        SiteIds = SplitCsv(row.SiteIdsCsv).Select(Guid.Parse).ToArray(),
        IsActive = row.IsActive,
        LastModifiedAtUtc = row.LastModifiedAtUtc,
        ETag = row.ETag,
    };

    private static IReadOnlyList<string> SplitCsv(string? csv) =>
        string.IsNullOrEmpty(csv)
            ? Array.Empty<string>()
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>Raw row shape returned by <c>[EnterpriseFabric].[usp_User_GetById]</c>.</summary>
    private record UserRow
    {
        public required Guid UserId { get; init; }
        public required Guid TenantId { get; init; }
        public required string Email { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public string? RolesCsv { get; init; }
        public string? SiteIdsCsv { get; init; }
        public required bool IsActive { get; init; }
        public required DateTimeOffset LastModifiedAtUtc { get; init; }
        public required string ETag { get; init; }
    }

    /// <summary>Raw row shape returned by <c>[EnterpriseFabric].[usp_User_List]</c>, including the windowed total row count.</summary>
    private sealed record UserListRow : UserRow
    {
        public required int TotalCount { get; init; }
    }
}
