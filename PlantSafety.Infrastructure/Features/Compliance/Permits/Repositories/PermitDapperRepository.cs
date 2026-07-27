// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.Compliance.Permits
//  Layer:          Infrastructure
//  File:           PermitDapperRepository.cs
//  Created:        2026-07-12
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Permit aggregate. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Features.Compliance.Handlers.Permits;

namespace PlantSafety.Infrastructure.Features.Compliance.Permits.Repositories;

/// <summary>
/// Dapper-backed repository for the <c>Permit</c> aggregate.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class PermitDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<PermitDapperRepository> logger)
    : IPermitRepository
{
    /// <inheritdoc/>
    public async Task CreateAsync(
        Guid permitId,
        Guid tenantId,
        Guid siteId,
        string permitType,
        string workDescription,
        string workLocation,
        Guid workLeadId,
        DateTimeOffset validFromUtc,
        DateTimeOffset validToUtc,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Create Permit {PermitId} for tenant {TenantId}", permitId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("PermitId", permitId)
            .AddGuid("SiteId", siteId)
            .AddString("PermitType", permitType, 100)
            .AddString("WorkDescription", workDescription)
            .AddString("WorkLocation", workLocation, 500)
            .AddGuid("WorkLeadId", workLeadId)
            .AddDateTime("ValidFromUtc", validFromUtc.UtcDateTime)
            .AddDateTime("ValidToUtc", validToUtc.UtcDateTime)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Compliance].[usp_Permit_Create]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task ApproveAsync(
        Guid tenantId,
        Guid permitId,
        string approvalNotes,
        string eTag,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Approve Permit {PermitId} for tenant {TenantId}", permitId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("PermitId", permitId)
            .AddString("ApprovalNotes", approvalNotes)
            .AddString("ETag", eTag, 64)
            .Build();

        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Compliance].[usp_Permit_Approve]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"Permit {permitId} was modified by another process or the ETag is stale.", correlationId);
        }

        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<PermitReadModel?> GetByIdAsync(Guid tenantId, Guid permitId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get Permit {PermitId} for tenant {TenantId}", permitId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("PermitId", permitId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<PermitReadModel>(
            "[Compliance].[usp_Permit_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<PermitReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? permitType,
        string? status,
        DateTimeOffset? validFromUtc,
        DateTimeOffset? validToUtc,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("List Permits for tenant {TenantId} page {PageNumber}", tenantId, pageNumber);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("PermitType", permitType, 100)
            .AddString("Status", status, 50)
            .AddDateTime("ValidFromUtc", validFromUtc?.UtcDateTime)
            .AddDateTime("ValidToUtc", validToUtc?.UtcDateTime)
            .AddString("OrderBy", orderBy, 100)
            .AddString("SortDirection", sortDirection, 4)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var rows = await queryExecutor.QueryAsync<PermitListRow>(
            "[Compliance].[usp_Permit_List]", parameters, cancellationToken);

        var items = rows
            .Select(r => new PermitReadModel
            {
                PermitId = r.PermitId,
                TenantId = r.TenantId,
                SiteId = r.SiteId,
                PermitType = r.PermitType,
                PermitNumber = r.PermitNumber,
                WorkDescription = r.WorkDescription,
                WorkLocation = r.WorkLocation,
                WorkLeadId = r.WorkLeadId,
                ValidFromUtc = r.ValidFromUtc,
                ValidToUtc = r.ValidToUtc,
                Status = r.Status,
                ApprovedByActorId = r.ApprovedByActorId,
                ApprovedAtUtc = r.ApprovedAtUtc,
                LastModifiedAtUtc = r.LastModifiedAtUtc,
                ETag = r.ETag,
            })
            .ToList();

        var totalCount = rows.Count > 0 ? rows[0].TotalCount : 0;

        return (items, totalCount);
    }

    /// <summary>Row shape returned by <c>[Compliance].[usp_Permit_List]</c>; carries a window-function TotalCount.</summary>
    private sealed record PermitListRow
    {
        public required Guid PermitId { get; init; }
        public required Guid TenantId { get; init; }
        public required Guid SiteId { get; init; }
        public required string PermitType { get; init; }
        public required string PermitNumber { get; init; }
        public required string WorkDescription { get; init; }
        public required string WorkLocation { get; init; }
        public required Guid WorkLeadId { get; init; }
        public required DateTimeOffset ValidFromUtc { get; init; }
        public required DateTimeOffset ValidToUtc { get; init; }
        public required string Status { get; init; }
        public required Guid? ApprovedByActorId { get; init; }
        public required DateTimeOffset? ApprovedAtUtc { get; init; }
        public required DateTimeOffset LastModifiedAtUtc { get; init; }
        public required string ETag { get; init; }
        public required int TotalCount { get; init; }
    }
}
