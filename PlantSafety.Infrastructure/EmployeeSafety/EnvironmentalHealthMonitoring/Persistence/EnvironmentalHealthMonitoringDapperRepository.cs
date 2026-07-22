// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
//  Layer:          Infrastructure
//  File:           EnvironmentalHealthMonitoringDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the EnvironmentalHealthMonitoring aggregate. Implements both the
//                  Domain-layer write repository (IEnvironmentalHealthMonitoringRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetEnvironmentalHealthMonitoringItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  EnvironmentalHealthMonitoring exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Specifications;
using PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.ValueObjects.Ids;
using PlantSafety.Application.EmployeeSafety.EnvironmentalHealthMonitoring.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Aggregates.EnvironmentalHealthMonitoring;
using IWriteRepo = PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Interfaces.IEnvironmentalHealthMonitoringRepository;
using IReadRepo = PlantSafety.Application.EmployeeSafety.EnvironmentalHealthMonitoring.Repositories.IEnvironmentalHealthMonitoringRepository;

namespace PlantSafety.Infrastructure.EmployeeSafety.EnvironmentalHealthMonitoring.Persistence;

/// <summary>Flat Dapper row for [EmployeeSafety].[EnvironmentalHealthMonitoring], column-for-column with the EnvironmentalHealthMonitoring aggregate.</summary>
internal sealed class EnvironmentalHealthMonitoringRow
{
    public Guid EnvironmentalHealthMonitoringId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? MonitoringTitle { get; init; }
    public string? Category { get; init; }
    public string? MonitoringLocation { get; init; }
    public Guid MonitoredByUserId { get; init; }
    public DateTime? MeasuredAtUtc { get; init; }
    public DateTime? NextMeasurementDueAtUtc { get; init; }
    public decimal? ExposureLevelScore { get; init; }
    public decimal ExceedanceThreshold { get; init; }
    public int ExceedanceCount { get; init; }
    public string? CorrectiveActionDescription { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the EnvironmentalHealthMonitoring aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class EnvironmentalHealthMonitoringDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<EnvironmentalHealthMonitoringDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "EmployeeSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(EnvironmentalHealthMonitoringId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for EnvironmentalHealthMonitoring {EnvironmentalHealthMonitoringId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("EnvironmentalHealthMonitoringId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<EnvironmentalHealthMonitoringRow>(
            $"[{Schema}].[usp_EnvironmentalHealthMonitoring_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, EnvironmentalHealthMonitoringRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(EnvironmentalHealthMonitoringSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for EnvironmentalHealthMonitoring");

        var rows = await queryExecutor.QueryAsync<EnvironmentalHealthMonitoringRow>(
            $"[{Schema}].[usp_EnvironmentalHealthMonitoring_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, EnvironmentalHealthMonitoringRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for EnvironmentalHealthMonitoring {EnvironmentalHealthMonitoringId}", aggregate.EnvironmentalHealthMonitoringId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("EnvironmentalHealthMonitoringId", aggregate, "EnvironmentalHealthMonitoringId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("MonitoringTitle", aggregate, "MonitoringTitle")
            .AddAggregateProperty("Category", aggregate, "Category")
            .AddAggregateProperty("MonitoringLocation", aggregate, "MonitoringLocation")
            .AddAggregateProperty("MonitoredByUserId", aggregate, "MonitoredByUserId")
            .AddAggregateProperty("MeasuredAtUtc", aggregate, "MeasuredAtUtc")
            .AddAggregateProperty("NextMeasurementDueAtUtc", aggregate, "NextMeasurementDueAtUtc")
            .AddAggregateProperty("ExposureLevelScore", aggregate, "ExposureLevelScore")
            .AddAggregateProperty("ExceedanceThreshold", aggregate, "ExceedanceThreshold")
            .AddAggregateProperty("ExceedanceCount", aggregate, "ExceedanceCount")
            .AddAggregateProperty("CorrectiveActionDescription", aggregate, "CorrectiveActionDescription")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_EnvironmentalHealthMonitoring_Create]", parameters, cancellationToken: cancellationToken);
    
        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events
        // raised by the aggregate during this write so TransactionalUnitOfWork.CommitAsync
        // (invoked by TransactionBehavior after the handler returns) can flush them to
        // [System].[OutboxMessages]. See TransactionalUnitOfWork remarks for the known
        // eventual-consistency gap (outbox flush is not in the same SQL transaction).
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
}

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for EnvironmentalHealthMonitoring {EnvironmentalHealthMonitoringId}", aggregate.EnvironmentalHealthMonitoringId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("EnvironmentalHealthMonitoringId", aggregate, "EnvironmentalHealthMonitoringId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("MonitoringTitle", aggregate, "MonitoringTitle")
            .AddAggregateProperty("Category", aggregate, "Category")
            .AddAggregateProperty("MonitoringLocation", aggregate, "MonitoringLocation")
            .AddAggregateProperty("MonitoredByUserId", aggregate, "MonitoredByUserId")
            .AddAggregateProperty("MeasuredAtUtc", aggregate, "MeasuredAtUtc")
            .AddAggregateProperty("NextMeasurementDueAtUtc", aggregate, "NextMeasurementDueAtUtc")
            .AddAggregateProperty("ExposureLevelScore", aggregate, "ExposureLevelScore")
            .AddAggregateProperty("ExceedanceThreshold", aggregate, "ExceedanceThreshold")
            .AddAggregateProperty("ExceedanceCount", aggregate, "ExceedanceCount")
            .AddAggregateProperty("CorrectiveActionDescription", aggregate, "CorrectiveActionDescription")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_EnvironmentalHealthMonitoring_Update]", parameters, cancellationToken: cancellationToken);
    }

    /// <inheritdoc cref="IReadRepo.GetEnvironmentalHealthMonitoringItemsAsync"/>
    public async Task<IReadOnlyList<EnvironmentalHealthMonitoringItemDto>> GetEnvironmentalHealthMonitoringItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetEnvironmentalHealthMonitoringItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EnvironmentalHealthMonitoringItemDto>(
            $"[{Schema}].[usp_EnvironmentalHealthMonitoring_GetItems]", parameters, cancellationToken);
    }
}
