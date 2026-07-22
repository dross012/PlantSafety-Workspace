// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.TwoHandControls
//  Layer:          Infrastructure
//  File:           TwoHandControlsDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the TwoHandControls aggregate. Implements both the
//                  Domain-layer write repository (ITwoHandControlsRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetTwoHandControlsItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  TwoHandControls exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.MachineSafety.TwoHandControls.Specifications;
using PlantSafety.Domain.MachineSafety.TwoHandControls.ValueObjects.Ids;
using PlantSafety.Application.MachineSafety.TwoHandControls.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.MachineSafety.TwoHandControls.Aggregates.TwoHandControls;
using IWriteRepo = PlantSafety.Domain.MachineSafety.TwoHandControls.Interfaces.ITwoHandControlsRepository;
using IReadRepo = PlantSafety.Application.MachineSafety.TwoHandControls.Repositories.ITwoHandControlsRepository;

namespace PlantSafety.Infrastructure.MachineSafety.TwoHandControls.Persistence;

/// <summary>Flat Dapper row for [MachineSafety].[TwoHandControls], column-for-column with the TwoHandControls aggregate.</summary>
internal sealed class TwoHandControlsRow
{
    public Guid TwoHandControlsId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? ControlTitle { get; init; }
    public string? EquipmentType { get; init; }
    public string? EquipmentLocation { get; init; }
    public Guid TestedByUserId { get; init; }
    public DateTime? TestedAtUtc { get; init; }
    public DateTime? NextTestDueAtUtc { get; init; }
    public int BypassAttemptCount { get; init; }
    public decimal? SynchronizationScore { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the TwoHandControls aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class TwoHandControlsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<TwoHandControlsDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "MachineSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(TwoHandControlsId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for TwoHandControls {TwoHandControlsId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TwoHandControlsId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<TwoHandControlsRow>(
            $"[{Schema}].[usp_TwoHandControls_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, TwoHandControlsRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(TwoHandControlsSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for TwoHandControls");

        var rows = await queryExecutor.QueryAsync<TwoHandControlsRow>(
            $"[{Schema}].[usp_TwoHandControls_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, TwoHandControlsRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for TwoHandControls {TwoHandControlsId}", aggregate.TwoHandControlsId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("TwoHandControlsId", aggregate, "TwoHandControlsId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ControlTitle", aggregate, "ControlTitle")
            .AddAggregateProperty("EquipmentType", aggregate, "EquipmentType")
            .AddAggregateProperty("EquipmentLocation", aggregate, "EquipmentLocation")
            .AddAggregateProperty("TestedByUserId", aggregate, "TestedByUserId")
            .AddAggregateProperty("TestedAtUtc", aggregate, "TestedAtUtc")
            .AddAggregateProperty("NextTestDueAtUtc", aggregate, "NextTestDueAtUtc")
            .AddAggregateProperty("BypassAttemptCount", aggregate, "BypassAttemptCount")
            .AddAggregateProperty("SynchronizationScore", aggregate, "SynchronizationScore")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_TwoHandControls_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for TwoHandControls {TwoHandControlsId}", aggregate.TwoHandControlsId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("TwoHandControlsId", aggregate, "TwoHandControlsId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ControlTitle", aggregate, "ControlTitle")
            .AddAggregateProperty("EquipmentType", aggregate, "EquipmentType")
            .AddAggregateProperty("EquipmentLocation", aggregate, "EquipmentLocation")
            .AddAggregateProperty("TestedByUserId", aggregate, "TestedByUserId")
            .AddAggregateProperty("TestedAtUtc", aggregate, "TestedAtUtc")
            .AddAggregateProperty("NextTestDueAtUtc", aggregate, "NextTestDueAtUtc")
            .AddAggregateProperty("BypassAttemptCount", aggregate, "BypassAttemptCount")
            .AddAggregateProperty("SynchronizationScore", aggregate, "SynchronizationScore")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_TwoHandControls_Update]", parameters, cancellationToken: cancellationToken);
    }

    /// <inheritdoc cref="IReadRepo.GetTwoHandControlsItemsAsync"/>
    public async Task<IReadOnlyList<TwoHandControlsItemDto>> GetTwoHandControlsItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetTwoHandControlsItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<TwoHandControlsItemDto>(
            $"[{Schema}].[usp_TwoHandControls_GetItems]", parameters, cancellationToken);
    }
}
