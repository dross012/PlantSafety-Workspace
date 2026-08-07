// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditFinding.OshaCoverage
//  Layer:          Infrastructure
//  File:           AuditFindingOshaCoverageDapperRepository.cs
//  Created:        2026-08-07
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for AuditFinding<->OSHARequirement coverage links. One
//                  stored-procedure call per pair (call volume is small -- a finding covers a
//                  handful of requirements, not thousands), each independently idempotent and
//                  tenant-scoped. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Auditing.AuditFinding.OshaCoverage.Repositories;

namespace PlantSafety.Infrastructure.Auditing.AuditFinding.OshaCoverage.Persistence;

/// <summary>Dapper-backed repository for Auditing.AuditFindingOshaCoverage links.</summary>
public sealed class AuditFindingOshaCoverageDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    ILogger<AuditFindingOshaCoverageDapperRepository> logger)
    : IAuditFindingOshaCoverageRepository
{
    /// <inheritdoc/>
    public async Task<int> LinkAsync(Guid tenantId, Guid auditFindingId, IReadOnlyCollection<Guid> oshaRequirementIds, Guid actorId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Linking AuditFinding {AuditFindingId} to {Count} OSHA requirements for Tenant {TenantId}",
            auditFindingId, oshaRequirementIds.Count, tenantId);

        using var connection = connectionFactory.CreateConnection();
        var linkedCount = 0;

        foreach (var oshaRequirementId in oshaRequirementIds.Distinct())
        {
            var parameters = new StoredProcedureBuilder()
                .AddGuid("AuditFindingOshaCoverageId", Guid.NewGuid())
                .AddGuid("TenantId", tenantId)
                .AddGuid("AuditFindingId", auditFindingId)
                .AddGuid("OSHARequirementId", oshaRequirementId)
                .AddGuid("CreatedBy", actorId)
                .Build();

            var rowsAffected = await commandExecutor.ExecuteAsync(
                connection, "[Auditing].[usp_AuditFindingOshaCoverage_Link]", parameters, cancellationToken: cancellationToken);

            if (rowsAffected > 0) linkedCount++;
        }

        return linkedCount;
    }
}
