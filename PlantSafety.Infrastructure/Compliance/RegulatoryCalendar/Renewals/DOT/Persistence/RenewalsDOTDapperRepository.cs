// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RenewalsDOT
//  Layer:          Infrastructure
//  File:           RenewalsDOTDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the RenewalsDOT read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.RegulatoryCalendar.Renewals.DOT.Models;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Renewals.DOT.Repositories;

namespace PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Renewals.DOT.Persistence;

/// <summary>Dapper-backed repository for <see cref="DOTItemDto"/> queries.</summary>
public sealed class RenewalsDOTDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<RenewalsDOTDapperRepository> logger)
    : IDOTRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<DOTItemDto>> GetRenewalsAsync(
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

        return await queryExecutor.QueryAsync<DOTItemDto>(
            "[Compliance].[usp_RenewalsDOT_List]", parameters, cancellationToken);
    }
}