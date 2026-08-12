// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.AccumulationAreas
//  Layer:          Infrastructure
//  File:           AccumulationAreaDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Dapper repository for the AccumulationArea aggregate. Implements both the
//                  Domain-layer write repository and the Application-layer read-model
//                  repository against the same table. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Specifications;
using PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.ValueObjects.Ids;
using PlantSafety.Application.HazardousWasteManagement.AccumulationAreas.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Aggregates.AccumulationArea;
using IWriteRepo = PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Interfaces.IAccumulationAreaRepository;
using IReadRepo = PlantSafety.Application.HazardousWasteManagement.AccumulationAreas.Repositories.IAccumulationAreaRepository;

namespace PlantSafety.Infrastructure.HazardousWasteManagement.AccumulationAreas.Persistence;

/// <summary>Flat Dapper row for [HazardousWasteManagement].[AccumulationAreas], column-for-column with the AccumulationArea aggregate.</summary>
internal sealed class AccumulationAreaRow
{
    public Guid AccumulationAreaId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? AreaName { get; init; }
    public string? LocationDescription { get; init; }
    public int TimeLimitDays { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>Dapper-backed repository for the AccumulationArea aggregate.</summary>
public sealed class AccumulationAreaDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "HazardousWasteManagement";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(AccumulationAreaId id, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("AccumulationAreaId", id.Value).Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<AccumulationAreaRow>(
            $"[{Schema}].[usp_AccumulationAreas_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, AccumulationAreaRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(AccumulationAreaSpecification specification, CancellationToken cancellationToken = default)
    {
        var rows = await queryExecutor.QueryAsync<AccumulationAreaRow>(
            $"[{Schema}].[usp_AccumulationAreas_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows.Select(AggregateHydrator.Hydrate<Agg, AccumulationAreaRow>).Where(predicate).ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("AccumulationAreaId", aggregate, "AccumulationAreaId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AreaName", aggregate, "AreaName")
            .AddAggregateProperty("LocationDescription", aggregate, "LocationDescription")
            .AddAggregateProperty("TimeLimitDays", aggregate, "TimeLimitDays")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_AccumulationAreas_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("AccumulationAreaId", aggregate, "AccumulationAreaId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_AccumulationAreas_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.ListAccumulationAreasNearingTimeLimitAsync"/>
    public async Task<IReadOnlyList<AccumulationAreaItemDto>> ListAccumulationAreasNearingTimeLimitAsync(
        Guid tenantId, Guid? siteId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .Build();

        return await queryExecutor.QueryAsync<AccumulationAreaItemDto>(
            $"[{Schema}].[usp_AccumulationAreas_ListNearingTimeLimit]", parameters, cancellationToken);
    }
}
