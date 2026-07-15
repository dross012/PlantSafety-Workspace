// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RenewalsISO
//  Layer:          Infrastructure
//  File:           ISODapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the RenewalsISO read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.RegulatoryCalendar.Renewals.ISO.Models;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Renewals.ISO.Repositories;

namespace PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Renewals.ISO.Persistence;

/// <summary>Dapper-backed repository for <see cref="ISOItemDto"/> queries.</summary>
public sealed class ISODapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ISODapperRepository> logger)
    : IISORepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ISOItemDto>> GetRenewalsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRenewalsAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ISOItemDto>(
            "[Compliance].[usp_RenewalsISO_List]", parameters, cancellationToken);
    }
}