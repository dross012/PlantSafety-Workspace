// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.ComplianceMonitoring.PermissionUsage
//  Layer:          Infrastructure
//  File:           PermissionUsageLogReadDapperRepository.cs
//  Created:        2026-08-03
//  Author:         Security & Compliance Agent (Phase 7 §5.4)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository implementing IPermissionUsageLogReadRepository against
//                  [Auditing].[PermissionUsageLog] via usp_PermissionUsageLog_GetByTenant, routed
//                  through the shared DapperQueryExecutor chokepoint (not a raw connection) so
//                  SESSION_CONTEXT('TenantId') is populated before the read -- this table carries a
//                  NOT NULL uniqueidentifier TenantId column, so it is automatically picked up by
//                  90-row-level-security.sql's live-discovery FILTER/BLOCK policy the moment that
//                  migration is applied to a real environment; reading it any other way would
//                  silently return zero rows under RLS. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Models;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Repositories;

namespace PlantSafety.Infrastructure.Auditing.ComplianceMonitoring.PermissionUsage.Persistence;

/// <summary>Dapper-backed <see cref="IPermissionUsageLogReadRepository"/> over <c>[Auditing].[PermissionUsageLog]</c>.</summary>
public sealed class PermissionUsageLogReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PermissionUsageLogReadDapperRepository> logger) : IPermissionUsageLogReadRepository
{
    /// <summary>Flat read-model row shape returned by <c>usp_PermissionUsageLog_GetByTenant</c> (Dapper requires an init-settable shape; <see cref="PermissionUsageLogEntry"/> is a positional record).</summary>
    private sealed record PermissionUsageLogRow
    {
        public required Guid TenantId { get; init; }
        public required Guid UserId { get; init; }
        public required string Permission { get; init; }
        public required string RequestType { get; init; }
        public required string Decision { get; init; }
        public required Guid CorrelationId { get; init; }
        public required DateTime OccurredAtUtc { get; init; }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PermissionUsageLogEntry>> GetByTenantAsync(
        Guid tenantId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetByTenant for PermissionUsageLog Tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddDateTime("FromUtc", fromUtc)
            .AddDateTime("ToUtc", toUtc)
            .Build();

        var rows = await queryExecutor.QueryAsync<PermissionUsageLogRow>(
            "[Auditing].[usp_PermissionUsageLog_GetByTenant]", parameters, cancellationToken);

        return rows
            .Select(r => new PermissionUsageLogEntry(
                r.TenantId, r.UserId, r.Permission, r.RequestType, r.Decision, r.CorrelationId, r.OccurredAtUtc))
            .ToList();
    }
}
