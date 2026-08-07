// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditFinding.NfpaCoverage
//  Layer:          Infrastructure
//  File:           AuditFindingNfpaCoverageDapperRepository.cs
//  Created:        2026-08-07
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for AuditFinding<->NFPA_HazardProfile coverage links. One
//                  stored-procedure call per pair, idempotent and tenant-scoped, mirroring
//                  AuditFindingOshaCoverageDapperRepository -- except usp_AuditFindingNfpaCoverage_Link
//                  is already single-hazard-per-call (no internal loop needed here; the caller,
//                  LinkAuditFindingToNfpaHazardsCommandHandler, loops once per hazard id).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Auditing.AuditFinding.NfpaCoverage.Repositories;

namespace PlantSafety.Infrastructure.Auditing.AuditFinding.NfpaCoverage.Persistence;

/// <summary>Dapper-backed repository for Auditing.AuditFindingNfpaCoverage links.</summary>
public sealed class AuditFindingNfpaCoverageDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    ILogger<AuditFindingNfpaCoverageDapperRepository> logger)
    : IAuditFindingNfpaCoverageRepository
{
    /// <inheritdoc/>
    public async Task<bool> LinkAsync(Guid tenantId, Guid auditFindingId, Guid hazardId, string? createdBy, CancellationToken cancellationToken)
    {
        logger.LogDebug("Linking AuditFinding {AuditFindingId} to NFPA hazard {HazardId} for Tenant {TenantId}",
            auditFindingId, hazardId, tenantId);

        using var connection = connectionFactory.CreateConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AuditFindingNfpaCoverageId", Guid.NewGuid())
            .AddGuid("TenantId", tenantId)
            .AddGuid("AuditFindingId", auditFindingId)
            .AddGuid("HazardId", hazardId)
            .AddString("CreatedBy", createdBy, 200)
            .Build();

        var rowsAffected = await commandExecutor.ExecuteAsync(
            connection, "[Auditing].[usp_AuditFindingNfpaCoverage_Link]", parameters, cancellationToken: cancellationToken);

        return rowsAffected > 0;
    }
}
