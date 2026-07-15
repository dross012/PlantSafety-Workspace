// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification.Contractor
//  Layer:          Infrastructure
//  File:           ContractorDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Certification.Contractor read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Certification.Contractor.Models;
using PlantSafety.Application.Compliance.Certification.Contractor.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Certification.Contractor.Persistence;

/// <summary>
/// Dapper-backed repository for Certification.Contractor read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class ContractorDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ContractorDapperRepository> logger)
    : IContractorRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ContractorItemDto>> GetContractorAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetContractor for Certification.Contractor tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ContractorItemDto>(
            "[Compliance].[usp_CertificationContractor_GetContractor]", parameters, cancellationToken);
    }
}
