// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Infrastructure
//  File:           CultureRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Culture aggregate. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using ICultureRepository = PlantSafety.Application.Common.Interfaces.Persistence.ICultureRepository;
using CultureAggregate = PlantSafety.Domain.Analytics.Culture.Aggregates.Culture;

namespace PlantSafety.Infrastructure.Analytics.Culture.Persistence;

/// <summary>Dapper-backed repository for <see cref="CultureReadModel"/> queries and the <see cref="CultureAggregate"/> write side.</summary>
public sealed class CultureRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<CultureRepository> logger) : ICultureRepository
{
    /// <inheritdoc/>
    public async Task AddAsync(CultureAggregate culture, CancellationToken cancellationToken)
    {
        logger.LogDebug("Add Culture {CultureId}", culture.CultureId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CultureId", culture.CultureId.Value)
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), culture.TenantId)
            .AddGuid("SiteId", culture.SiteId)
            .AddString("Status", culture.Status.ToString(), 30)
            .AddString("SurveyPeriod", culture.SurveyPeriod, 20)
            .AddDecimal("CultureIndexScore", culture.CultureIndexScore)
            .AddDecimal("EngagementScore", culture.EngagementScore)
            .AddDecimal("ParticipationRate", culture.ParticipationRate)
            .AddString("TopRiskTheme", culture.TopRiskTheme, 200)
            .AddDateTime("CreatedAtUtc", culture.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", culture.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Analytics].[usp_Culture_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in culture.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, culture.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        culture.ClearDomainEvents();
    }

    /// <summary>Returns the Culture read model for the specified identifier, or <c>null</c> if not found.</summary>
    public async Task<CultureReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Culture {Id}", id);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), tenantId)
            .AddGuid("CultureId", id)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<CultureReadModel>(
            $"[Analytics].[usp_Culture_GetById]", parameters);
    }

    /// <summary>Returns all Culture read models for the specified tenant.</summary>
    public async Task<IReadOnlyList<CultureReadModel>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetAll for Culture tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), tenantId)
            .Build();

        var result = await queryExecutor.QueryAsync<CultureReadModel>(
            $"[Analytics].[usp_Culture_GetAll]", parameters);

        return result.ToList().AsReadOnly();
    }
}
