// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RenewalsTraining
//  Layer:          Infrastructure
//  File:           TrainingDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the RenewalsTraining read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.RegulatoryCalendar.Renewals.Training.Models;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Renewals.Training.Repositories;

namespace PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Renewals.Training.Persistence;

/// <summary>Dapper-backed repository for <see cref="TrainingItemDto"/> queries.</summary>
public sealed class TrainingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<TrainingDapperRepository> logger)
    : ITrainingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<TrainingItemDto>> GetRenewalsAsync(
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

        return await queryExecutor.QueryAsync<TrainingItemDto>(
            "[Compliance].[usp_RenewalsTraining_List]", parameters, cancellationToken);
    }
}