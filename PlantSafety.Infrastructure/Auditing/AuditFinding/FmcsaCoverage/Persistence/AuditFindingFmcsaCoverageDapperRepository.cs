// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditFinding.FmcsaCoverage
//  Layer:          Infrastructure
//  File:           AuditFindingFmcsaCoverageDapperRepository.cs
//  Created:        2026-08-07
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for AuditFinding<->FMCSARequirement coverage links. One
//                  stored-procedure call per pair -- usp_AuditFindingFmcsaCoverage_Link is
//                  single-requirement-per-call, mirroring AuditFindingNfpaCoverageDapperRepository
//                  (the caller, LinkAuditFindingToFmcsaRequirementsCommandHandler, loops once per
//                  requirement id). Idempotent and tenant-scoped. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Auditing.AuditFinding.FmcsaCoverage.Repositories;

namespace PlantSafety.Infrastructure.Auditing.AuditFinding.FmcsaCoverage.Persistence;

/// <summary>Dapper-backed repository for Auditing.AuditFindingFmcsaCoverage links.</summary>
public sealed class AuditFindingFmcsaCoverageDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    ILogger<AuditFindingFmcsaCoverageDapperRepository> logger)
    : IAuditFindingFmcsaCoverageRepository
{
    /// <inheritdoc/>
    public async Task<bool> LinkAsync(Guid tenantId, Guid auditFindingId, Guid requirementId, Guid createdBy, CancellationToken cancellationToken)
    {
        logger.LogDebug("Linking AuditFinding {AuditFindingId} to FMCSA requirement {RequirementId} for Tenant {TenantId}",
            auditFindingId, requirementId, tenantId);

        using var connection = connectionFactory.CreateConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AuditFindingFmcsaCoverageId", Guid.NewGuid())
            .AddGuid("TenantId", tenantId)
            .AddGuid("AuditFindingId", auditFindingId)
            .AddGuid("RequirementId", requirementId)
            .AddGuid("CreatedBy", createdBy)
            .Build();

        var rowsAffected = await commandExecutor.ExecuteAsync(
            connection, "[Auditing].[usp_AuditFindingFmcsaCoverage_Link]", parameters, cancellationToken: cancellationToken);

        return rowsAffected > 0;
    }
}
