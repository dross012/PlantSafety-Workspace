// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePoint
//  Layer:          Infrastructure
//  File:           DischargePointDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the DischargePoint aggregate. Implements both the
//                  Domain write repository and the Application read-model repository against
//                  the same table. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Specifications;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.ValueObjects.Ids;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Aggregates.DischargePoint;
using IWriteRepo = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Interfaces.IDischargePointRepository;
using IReadRepo = PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Repositories.IDischargePointRepository;

namespace PlantSafety.Infrastructure.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Persistence;

/// <summary>Flat Dapper row for [EnvironmentalHealth].[DischargePoint], column-for-column with the aggregate.</summary>
internal sealed class DischargePointRow
{
    public Guid DischargePointId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public Guid DischargePermitId { get; init; }
    public string? PointCode { get; init; }
    public string? PointName { get; init; }
    public string? ReceivingWaterBody { get; init; }
    public string? Outfall { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>Dapper-backed repository for the DischargePoint aggregate.</summary>
public sealed class DischargePointDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "EnvironmentalHealth";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(DischargePointId id, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("DischargePointId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<DischargePointRow>(
            $"[{Schema}].[usp_DischargePoint_GetById]", parameters, cancellationToken);
        return row is null ? null : AggregateHydrator.Hydrate<Agg, DischargePointRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(DischargePointSpecification specification, CancellationToken cancellationToken = default)
    {
        var rows = await queryExecutor.QueryAsync<DischargePointRow>(
            $"[{Schema}].[usp_DischargePoint_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);
        var predicate = specification.ToPredicate();
        return rows.Select(AggregateHydrator.Hydrate<Agg, DischargePointRow>).Where(predicate).ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("DischargePointId", aggregate, "DischargePointId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("DischargePermitId", aggregate, "DischargePermitId")
            .AddAggregateProperty("PointCode", aggregate, "PointCode")
            .AddAggregateProperty("PointName", aggregate, "PointName")
            .AddAggregateProperty("ReceivingWaterBody", aggregate, "ReceivingWaterBody")
            .AddAggregateProperty("Outfall", aggregate, "Outfall")
            .AddAggregateProperty("Latitude", aggregate, "Latitude")
            .AddAggregateProperty("Longitude", aggregate, "Longitude")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_DischargePoint_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("DischargePointId", aggregate, "DischargePointId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("PointName", aggregate, "PointName")
            .AddAggregateProperty("ReceivingWaterBody", aggregate, "ReceivingWaterBody")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_DischargePoint_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.ListDischargePointsBySiteAsync"/>
    public async Task<IReadOnlyList<DischargePointItemDto>> ListDischargePointsBySiteAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).AddGuid("SiteId", siteId).Build();
        return await queryExecutor.QueryAsync<DischargePointItemDto>($"[{Schema}].[usp_DischargePoint_GetItems]", parameters, cancellationToken);
    }
}