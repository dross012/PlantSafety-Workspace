// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.LoadSecurementChecks
//  Layer:          Infrastructure
//  File:           LoadSecurementCheckDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the LoadSecurementCheck aggregate. Implements both the
//                  Domain-layer write repository (ILoadSecurementCheckRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (ListLoadSecurementFailuresAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  LoadSecurementCheck exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Specifications;
using PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.ValueObjects.Ids;
using PlantSafety.Application.LogisticsTransportSafety.LoadSecurementChecks.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Aggregates.LoadSecurementCheck;
using IWriteRepo = PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Interfaces.ILoadSecurementCheckRepository;
using IReadRepo = PlantSafety.Application.LogisticsTransportSafety.LoadSecurementChecks.Repositories.ILoadSecurementCheckRepository;

namespace PlantSafety.Infrastructure.LogisticsTransportSafety.LoadSecurementChecks.Persistence;

/// <summary>Flat Dapper row for [LogisticsTransportSafety].[LoadSecurementChecks], column-for-column with the LoadSecurementCheck aggregate.</summary>
internal sealed class LoadSecurementCheckRow
{
    public Guid LoadSecurementCheckId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public Guid? TripId { get; init; }
    public Guid VehicleId { get; init; }
    public Guid CheckedByUserId { get; init; }
    public DateTime? CheckedAtUtc { get; init; }
    public int DeficiencyCount { get; init; }
    public decimal SecurementScore { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the LoadSecurementCheck aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class LoadSecurementCheckDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<LoadSecurementCheckDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "LogisticsTransportSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(LoadSecurementCheckId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for LoadSecurementCheck {LoadSecurementCheckId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("LoadSecurementCheckId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<LoadSecurementCheckRow>(
            $"[{Schema}].[usp_LoadSecurementCheck_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, LoadSecurementCheckRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(LoadSecurementCheckSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for LoadSecurementCheck");

        var rows = await queryExecutor.QueryAsync<LoadSecurementCheckRow>(
            $"[{Schema}].[usp_LoadSecurementCheck_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, LoadSecurementCheckRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for LoadSecurementCheck {LoadSecurementCheckId}", aggregate.LoadSecurementCheckId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("LoadSecurementCheckId", aggregate, "LoadSecurementCheckId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("TripId", aggregate, "TripId")
            .AddAggregateProperty("VehicleId", aggregate, "VehicleId")
            .AddAggregateProperty("CheckedByUserId", aggregate, "CheckedByUserId")
            .AddAggregateProperty("CheckedAtUtc", aggregate, "CheckedAtUtc")
            .AddAggregateProperty("DeficiencyCount", aggregate, "DeficiencyCount")
            .AddAggregateProperty("SecurementScore", aggregate, "SecurementScore")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_LoadSecurementCheck_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for LoadSecurementCheck {LoadSecurementCheckId}", aggregate.LoadSecurementCheckId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("LoadSecurementCheckId", aggregate, "LoadSecurementCheckId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_LoadSecurementCheck_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.ListLoadSecurementFailuresAsync"/>
    public async Task<IReadOnlyList<LoadSecurementCheckItemDto>> ListLoadSecurementFailuresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("ListLoadSecurementFailuresAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<LoadSecurementCheckItemDto>(
            $"[{Schema}].[usp_LoadSecurementCheck_ListFailures]", parameters, cancellationToken);
    }
}
