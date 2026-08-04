// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportVehicles
//  Layer:          Infrastructure
//  File:           TransportVehicleDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the TransportVehicle aggregate. Implements both the
//                  Domain-layer write repository (ITransportVehicleRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetTransportVehicleByIdAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  TransportVehicle exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Specifications;
using PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.ValueObjects.Ids;
using PlantSafety.Application.LogisticsTransportSafety.TransportVehicles.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Aggregates.TransportVehicle;
using IWriteRepo = PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Interfaces.ITransportVehicleRepository;
using IReadRepo = PlantSafety.Application.LogisticsTransportSafety.TransportVehicles.Repositories.ITransportVehicleRepository;

namespace PlantSafety.Infrastructure.LogisticsTransportSafety.TransportVehicles.Persistence;

/// <summary>Flat Dapper row for [LogisticsTransportSafety].[TransportVehicles], column-for-column with the TransportVehicle aggregate.</summary>
internal sealed class TransportVehicleRow
{
    public Guid TransportVehicleId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? VehicleTitle { get; init; }
    public string? VehicleType { get; init; }
    public string? VehicleIdentifier { get; init; }
    public DateTime? LastInspectionAtUtc { get; init; }
    public DateTime? NextInspectionDueAtUtc { get; init; }
    public decimal? LastInspectionScore { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the TransportVehicle aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class TransportVehicleDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<TransportVehicleDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "LogisticsTransportSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(TransportVehicleId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for TransportVehicle {TransportVehicleId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TransportVehicleId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<TransportVehicleRow>(
            $"[{Schema}].[usp_TransportVehicle_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, TransportVehicleRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(TransportVehicleSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for TransportVehicle");

        var rows = await queryExecutor.QueryAsync<TransportVehicleRow>(
            $"[{Schema}].[usp_TransportVehicle_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, TransportVehicleRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for TransportVehicle {TransportVehicleId}", aggregate.TransportVehicleId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("TransportVehicleId", aggregate, "TransportVehicleId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("VehicleTitle", aggregate, "VehicleTitle")
            .AddAggregateProperty("VehicleType", aggregate, "VehicleType")
            .AddAggregateProperty("VehicleIdentifier", aggregate, "VehicleIdentifier")
            .AddAggregateProperty("LastInspectionAtUtc", aggregate, "LastInspectionAtUtc")
            .AddAggregateProperty("NextInspectionDueAtUtc", aggregate, "NextInspectionDueAtUtc")
            .AddAggregateProperty("LastInspectionScore", aggregate, "LastInspectionScore")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_TransportVehicle_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for TransportVehicle {TransportVehicleId}", aggregate.TransportVehicleId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("TransportVehicleId", aggregate, "TransportVehicleId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("VehicleTitle", aggregate, "VehicleTitle")
            .AddAggregateProperty("VehicleType", aggregate, "VehicleType")
            .AddAggregateProperty("VehicleIdentifier", aggregate, "VehicleIdentifier")
            .AddAggregateProperty("LastInspectionAtUtc", aggregate, "LastInspectionAtUtc")
            .AddAggregateProperty("NextInspectionDueAtUtc", aggregate, "NextInspectionDueAtUtc")
            .AddAggregateProperty("LastInspectionScore", aggregate, "LastInspectionScore")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_TransportVehicle_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetTransportVehicleByIdAsync"/>
    public async Task<TransportVehicleItemDto?> GetTransportVehicleByIdAsync(
        Guid tenantId,
        Guid transportVehicleId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetTransportVehicleByIdAsync for TransportVehicle {TransportVehicleId}", transportVehicleId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("TransportVehicleId", transportVehicleId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<TransportVehicleItemDto>(
            $"[{Schema}].[usp_TransportVehicle_GetItem]", parameters, cancellationToken);
    }
}
