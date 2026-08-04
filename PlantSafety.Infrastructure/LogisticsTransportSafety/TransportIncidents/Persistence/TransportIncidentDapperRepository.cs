// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportIncidents
//  Layer:          Infrastructure
//  File:           TransportIncidentDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the TransportIncident aggregate. Implements both the
//                  Domain-layer write repository (ITransportIncidentRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetTransportIncidentHistoryAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  TransportIncident exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Specifications;
using PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.ValueObjects.Ids;
using PlantSafety.Application.LogisticsTransportSafety.TransportIncidents.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Aggregates.TransportIncident;
using IWriteRepo = PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Interfaces.ITransportIncidentRepository;
using IReadRepo = PlantSafety.Application.LogisticsTransportSafety.TransportIncidents.Repositories.ITransportIncidentRepository;

namespace PlantSafety.Infrastructure.LogisticsTransportSafety.TransportIncidents.Persistence;

/// <summary>Flat Dapper row for [LogisticsTransportSafety].[TransportIncidents], column-for-column with the TransportIncident aggregate.</summary>
internal sealed class TransportIncidentRow
{
    public Guid TransportIncidentId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public Guid? TripId { get; init; }
    public Guid VehicleId { get; init; }
    public Guid DriverUserId { get; init; }
    public string? IncidentType { get; init; }
    public string? Severity { get; init; }
    public DateTime? OccurredAtUtc { get; init; }
    public string? Location { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the TransportIncident aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class TransportIncidentDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<TransportIncidentDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "LogisticsTransportSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(TransportIncidentId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for TransportIncident {TransportIncidentId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TransportIncidentId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<TransportIncidentRow>(
            $"[{Schema}].[usp_TransportIncident_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, TransportIncidentRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(TransportIncidentSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for TransportIncident");

        var rows = await queryExecutor.QueryAsync<TransportIncidentRow>(
            $"[{Schema}].[usp_TransportIncident_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, TransportIncidentRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for TransportIncident {TransportIncidentId}", aggregate.TransportIncidentId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("TransportIncidentId", aggregate, "TransportIncidentId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("TripId", aggregate, "TripId")
            .AddAggregateProperty("VehicleId", aggregate, "VehicleId")
            .AddAggregateProperty("DriverUserId", aggregate, "DriverUserId")
            .AddAggregateProperty("IncidentType", aggregate, "IncidentType")
            .AddAggregateProperty("Severity", aggregate, "Severity")
            .AddAggregateProperty("OccurredAtUtc", aggregate, "OccurredAtUtc")
            .AddAggregateProperty("Location", aggregate, "Location")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_TransportIncident_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for TransportIncident {TransportIncidentId}", aggregate.TransportIncidentId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("TransportIncidentId", aggregate, "TransportIncidentId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_TransportIncident_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetTransportIncidentHistoryAsync"/>
    public async Task<IReadOnlyList<TransportIncidentItemDto>> GetTransportIncidentHistoryAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetTransportIncidentHistoryAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<TransportIncidentItemDto>(
            $"[{Schema}].[usp_TransportIncident_GetHistory]", parameters, cancellationToken);
    }
}
