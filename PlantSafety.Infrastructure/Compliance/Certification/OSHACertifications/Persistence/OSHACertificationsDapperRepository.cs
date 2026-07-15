// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification.OSHACertifications
//  Layer:          Infrastructure
//  File:           OSHACertificationsDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Certification.OSHACertifications read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Certification.OSHACertifications.Models;
using PlantSafety.Application.Compliance.Certification.OSHACertifications.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Certification.OSHACertifications.Persistence;

/// <summary>
/// Dapper-backed repository for Certification.OSHACertifications read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class OSHACertificationsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<OSHACertificationsDapperRepository> logger)
    : IOSHACertificationsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<OSHACertificationsItemDto>> GetOSHACertificationsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetOSHACertifications for Certification.OSHACertifications tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<OSHACertificationsItemDto>(
            "[Compliance].[usp_CertificationOSHACertifications_GetOSHACertifications]", parameters, cancellationToken);
    }
}
