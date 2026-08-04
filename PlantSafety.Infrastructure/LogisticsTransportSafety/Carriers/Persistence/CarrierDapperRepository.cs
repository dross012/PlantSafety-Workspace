// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.Carriers
//  Layer:          Infrastructure
//  File:           CarrierDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Carrier aggregate. Implements both the Domain-layer
//                  write repository (ICarrierRepository, GetByIdAsync/ListAsync/AddAsync/UpdateAsync)
//                  and the Application-layer read-model repository of the same simple name
//                  (GetCarrierItemsAsync). All DB access via stored procedures; no dynamic SQL.
//                  Aggregate rehydration uses AggregateHydrator (reflection, confined to
//                  Infrastructure) because Carrier exposes only a private parameterless
//                  constructor and private-set properties with no public Domain-layer
//                  rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.LogisticsTransportSafety.Carriers.Specifications;
using PlantSafety.Domain.LogisticsTransportSafety.Carriers.ValueObjects.Ids;
using PlantSafety.Application.LogisticsTransportSafety.Carriers.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.LogisticsTransportSafety.Carriers.Aggregates.Carrier;
using IWriteRepo = PlantSafety.Domain.LogisticsTransportSafety.Carriers.Interfaces.ICarrierRepository;
using IReadRepo = PlantSafety.Application.LogisticsTransportSafety.Carriers.Repositories.ICarrierRepository;

namespace PlantSafety.Infrastructure.LogisticsTransportSafety.Carriers.Persistence;

/// <summary>Flat Dapper row for [LogisticsTransportSafety].[Carriers], column-for-column with the Carrier aggregate.</summary>
internal sealed class CarrierRow
{
    public Guid CarrierId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? CarrierName { get; init; }
    public string? CarrierType { get; init; }
    public DateTime? InsuranceExpiryAtUtc { get; init; }
    public decimal? SafetyRatingScore { get; init; }
    public DateTime? QualifiedAtUtc { get; init; }
    public string? SuspensionReason { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the Carrier aggregate. Implements the Domain write repository
/// and the Application read-model repository against the same table.
/// </summary>
public sealed class CarrierDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<CarrierDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "LogisticsTransportSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(CarrierId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Carrier {CarrierId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CarrierId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<CarrierRow>(
            $"[{Schema}].[usp_Carrier_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, CarrierRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(CarrierSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for Carrier");

        var rows = await queryExecutor.QueryAsync<CarrierRow>(
            $"[{Schema}].[usp_Carrier_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, CarrierRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for Carrier {CarrierId}", aggregate.CarrierId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("CarrierId", aggregate, "CarrierId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("CarrierName", aggregate, "CarrierName")
            .AddAggregateProperty("CarrierType", aggregate, "CarrierType")
            .AddAggregateProperty("InsuranceExpiryAtUtc", aggregate, "InsuranceExpiryAtUtc")
            .AddAggregateProperty("SafetyRatingScore", aggregate, "SafetyRatingScore")
            .AddAggregateProperty("QualifiedAtUtc", aggregate, "QualifiedAtUtc")
            .AddAggregateProperty("SuspensionReason", aggregate, "SuspensionReason")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_Carrier_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for Carrier {CarrierId}", aggregate.CarrierId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("CarrierId", aggregate, "CarrierId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("CarrierName", aggregate, "CarrierName")
            .AddAggregateProperty("CarrierType", aggregate, "CarrierType")
            .AddAggregateProperty("InsuranceExpiryAtUtc", aggregate, "InsuranceExpiryAtUtc")
            .AddAggregateProperty("SafetyRatingScore", aggregate, "SafetyRatingScore")
            .AddAggregateProperty("QualifiedAtUtc", aggregate, "QualifiedAtUtc")
            .AddAggregateProperty("SuspensionReason", aggregate, "SuspensionReason")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_Carrier_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetCarrierItemsAsync"/>
    public async Task<IReadOnlyList<CarrierItemDto>> GetCarrierItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetCarrierItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<CarrierItemDto>(
            $"[{Schema}].[usp_Carrier_GetItems]", parameters, cancellationToken);
    }
}
