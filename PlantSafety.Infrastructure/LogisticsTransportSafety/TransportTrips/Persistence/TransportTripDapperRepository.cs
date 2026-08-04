// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportTrips
//  Layer:          Infrastructure
//  File:           TransportTripDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the TransportTrip aggregate. Implements both the
//                  Domain-layer write repository (ITransportTripRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (ListActiveTripsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  TransportTrip exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Specifications;
using PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.ValueObjects.Ids;
using PlantSafety.Application.LogisticsTransportSafety.TransportTrips.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Aggregates.TransportTrip;
using IWriteRepo = PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Interfaces.ITransportTripRepository;
using IReadRepo = PlantSafety.Application.LogisticsTransportSafety.TransportTrips.Repositories.ITransportTripRepository;

namespace PlantSafety.Infrastructure.LogisticsTransportSafety.TransportTrips.Persistence;

/// <summary>Flat Dapper row for [LogisticsTransportSafety].[TransportTrips], column-for-column with the TransportTrip aggregate.</summary>
internal sealed class TransportTripRow
{
    public Guid TransportTripId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public Guid VehicleId { get; init; }
    public Guid DriverUserId { get; init; }
    public string? TripType { get; init; }
    public string? RouteDescription { get; init; }
    public DateTime? PlannedDepartureAtUtc { get; init; }
    public DateTime? PlannedArrivalAtUtc { get; init; }
    public DateTime? ActualDepartureAtUtc { get; init; }
    public DateTime? ActualArrivalAtUtc { get; init; }
    public decimal? DistanceMiles { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the TransportTrip aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class TransportTripDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<TransportTripDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "LogisticsTransportSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(TransportTripId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for TransportTrip {TransportTripId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TransportTripId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<TransportTripRow>(
            $"[{Schema}].[usp_TransportTrip_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, TransportTripRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(TransportTripSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for TransportTrip");

        var rows = await queryExecutor.QueryAsync<TransportTripRow>(
            $"[{Schema}].[usp_TransportTrip_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, TransportTripRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for TransportTrip {TransportTripId}", aggregate.TransportTripId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("TransportTripId", aggregate, "TransportTripId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("VehicleId", aggregate, "VehicleId")
            .AddAggregateProperty("DriverUserId", aggregate, "DriverUserId")
            .AddAggregateProperty("TripType", aggregate, "TripType")
            .AddAggregateProperty("RouteDescription", aggregate, "RouteDescription")
            .AddAggregateProperty("PlannedDepartureAtUtc", aggregate, "PlannedDepartureAtUtc")
            .AddAggregateProperty("PlannedArrivalAtUtc", aggregate, "PlannedArrivalAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_TransportTrip_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for TransportTrip {TransportTripId}", aggregate.TransportTripId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("TransportTripId", aggregate, "TransportTripId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ActualDepartureAtUtc", aggregate, "ActualDepartureAtUtc")
            .AddAggregateProperty("ActualArrivalAtUtc", aggregate, "ActualArrivalAtUtc")
            .AddAggregateProperty("DistanceMiles", aggregate, "DistanceMiles")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_TransportTrip_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.ListActiveTripsAsync"/>
    public async Task<IReadOnlyList<TransportTripItemDto>> ListActiveTripsAsync(
        Guid tenantId,
        Guid? siteId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("ListActiveTripsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .Build();

        return await queryExecutor.QueryAsync<TransportTripItemDto>(
            $"[{Schema}].[usp_TransportTrip_ListActive]", parameters, cancellationToken);
    }
}
