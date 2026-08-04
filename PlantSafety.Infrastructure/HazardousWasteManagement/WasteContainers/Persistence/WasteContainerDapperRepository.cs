// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteContainers
//  Layer:          Infrastructure
//  File:           WasteContainerDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Dapper repository for the WasteContainer aggregate. Implements both the
//                  Domain-layer write repository and the Application-layer read-model
//                  repository against the same table. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Specifications;
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.ValueObjects.Ids;
using PlantSafety.Application.HazardousWasteManagement.WasteContainers.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Aggregates.WasteContainer;
using IWriteRepo = PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Interfaces.IWasteContainerRepository;
using IReadRepo = PlantSafety.Application.HazardousWasteManagement.WasteContainers.Repositories.IWasteContainerRepository;

namespace PlantSafety.Infrastructure.HazardousWasteManagement.WasteContainers.Persistence;

/// <summary>Flat Dapper row for [HazardousWasteManagement].[WasteContainers], column-for-column with the WasteContainer aggregate.</summary>
internal sealed class WasteContainerRow
{
    public Guid WasteContainerId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public Guid AccumulationAreaId { get; init; }
    public Guid WasteStreamId { get; init; }
    public string? ContainerLabel { get; init; }
    public decimal CapacityGallons { get; init; }
    public int RegulatoryTimeLimitDays { get; init; }
    public DateTime? OpenedAtUtc { get; init; }
    public DateTime? ClosedAtUtc { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>Dapper-backed repository for the WasteContainer aggregate.</summary>
public sealed class WasteContainerDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<WasteContainerDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "HazardousWasteManagement";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(WasteContainerId id, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("WasteContainerId", id.Value).Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<WasteContainerRow>(
            $"[{Schema}].[usp_WasteContainers_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, WasteContainerRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(WasteContainerSpecification specification, CancellationToken cancellationToken = default)
    {
        var rows = await queryExecutor.QueryAsync<WasteContainerRow>(
            $"[{Schema}].[usp_WasteContainers_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows.Select(AggregateHydrator.Hydrate<Agg, WasteContainerRow>).Where(predicate).ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("WasteContainerId", aggregate, "WasteContainerId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AccumulationAreaId", aggregate, "AccumulationAreaId")
            .AddAggregateProperty("WasteStreamId", aggregate, "WasteStreamId")
            .AddAggregateProperty("ContainerLabel", aggregate, "ContainerLabel")
            .AddAggregateProperty("CapacityGallons", aggregate, "CapacityGallons")
            .AddAggregateProperty("RegulatoryTimeLimitDays", aggregate, "RegulatoryTimeLimitDays")
            .AddAggregateProperty("OpenedAtUtc", aggregate, "OpenedAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_WasteContainers_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("WasteContainerId", aggregate, "WasteContainerId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ClosedAtUtc", aggregate, "ClosedAtUtc")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_WasteContainers_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.ListContainersByAccumulationAreaAsync"/>
    public async Task<IReadOnlyList<WasteContainerItemDto>> ListContainersByAccumulationAreaAsync(
        Guid tenantId, Guid accumulationAreaId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("AccumulationAreaId", accumulationAreaId)
            .Build();

        return await queryExecutor.QueryAsync<WasteContainerItemDto>(
            $"[{Schema}].[usp_WasteContainers_ListByArea]", parameters, cancellationToken);
    }
}
