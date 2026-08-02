// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.LotoSystems
//  Layer:          Infrastructure
//  File:           LotoSystemsDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the LotoSystems aggregate. Implements both the
//                  Domain-layer write repository (ILotoSystemsRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetLotoSystemsItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  LotoSystems exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.MachineSafety.LotoSystems.Specifications;
using PlantSafety.Domain.MachineSafety.LotoSystems.ValueObjects.Ids;
using PlantSafety.Application.MachineSafety.LotoSystems.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.MachineSafety.LotoSystems.Aggregates.LotoSystems;
using IWriteRepo = PlantSafety.Domain.MachineSafety.LotoSystems.Interfaces.ILotoSystemsRepository;
using IReadRepo = PlantSafety.Application.MachineSafety.LotoSystems.Repositories.ILotoSystemsRepository;

namespace PlantSafety.Infrastructure.MachineSafety.LotoSystems.Persistence;

/// <summary>Flat Dapper row for [MachineSafety].[LotoSystems], column-for-column with the LotoSystems aggregate.</summary>
internal sealed class LotoSystemsRow
{
    public Guid LotoSystemsId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? SystemTitle { get; init; }
    public string? EquipmentTag { get; init; }
    public string? EnergySourceTypes { get; init; }
    public string? IsolationPointDescription { get; init; }
    public int IsolationPointCount { get; init; }
    public Guid VerifiedByUserId { get; init; }
    public DateTime? VerifiedZeroEnergyAtUtc { get; init; }
    public DateTime? AuditedAtUtc { get; init; }
    public decimal? ComplianceScore { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the LotoSystems aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class LotoSystemsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<LotoSystemsDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "MachineSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(LotoSystemsId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for LotoSystems {LotoSystemsId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("LotoSystemsId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<LotoSystemsRow>(
            $"[{Schema}].[usp_LotoSystems_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, LotoSystemsRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(LotoSystemsSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for LotoSystems");

        var rows = await queryExecutor.QueryAsync<LotoSystemsRow>(
            $"[{Schema}].[usp_LotoSystems_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, LotoSystemsRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for LotoSystems {LotoSystemsId}", aggregate.LotoSystemsId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("LotoSystemsId", aggregate, "LotoSystemsId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("SystemTitle", aggregate, "SystemTitle")
            .AddAggregateProperty("EquipmentTag", aggregate, "EquipmentTag")
            .AddAggregateProperty("EnergySourceTypes", aggregate, "EnergySourceTypes")
            .AddAggregateProperty("IsolationPointDescription", aggregate, "IsolationPointDescription")
            .AddAggregateProperty("IsolationPointCount", aggregate, "IsolationPointCount")
            .AddAggregateProperty("VerifiedByUserId", aggregate, "VerifiedByUserId")
            .AddAggregateProperty("VerifiedZeroEnergyAtUtc", aggregate, "VerifiedZeroEnergyAtUtc")
            .AddAggregateProperty("AuditedAtUtc", aggregate, "AuditedAtUtc")
            .AddAggregateProperty("ComplianceScore", aggregate, "ComplianceScore")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_LotoSystems_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for LotoSystems {LotoSystemsId}", aggregate.LotoSystemsId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("LotoSystemsId", aggregate, "LotoSystemsId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("SystemTitle", aggregate, "SystemTitle")
            .AddAggregateProperty("EquipmentTag", aggregate, "EquipmentTag")
            .AddAggregateProperty("EnergySourceTypes", aggregate, "EnergySourceTypes")
            .AddAggregateProperty("IsolationPointDescription", aggregate, "IsolationPointDescription")
            .AddAggregateProperty("IsolationPointCount", aggregate, "IsolationPointCount")
            .AddAggregateProperty("VerifiedByUserId", aggregate, "VerifiedByUserId")
            .AddAggregateProperty("VerifiedZeroEnergyAtUtc", aggregate, "VerifiedZeroEnergyAtUtc")
            .AddAggregateProperty("AuditedAtUtc", aggregate, "AuditedAtUtc")
            .AddAggregateProperty("ComplianceScore", aggregate, "ComplianceScore")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_LotoSystems_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetLotoSystemsItemsAsync"/>
    public async Task<IReadOnlyList<LotoSystemsItemDto>> GetLotoSystemsItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetLotoSystemsItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<LotoSystemsItemDto>(
            $"[{Schema}].[usp_LotoSystems_GetItems]", parameters, cancellationToken);
    }
}
