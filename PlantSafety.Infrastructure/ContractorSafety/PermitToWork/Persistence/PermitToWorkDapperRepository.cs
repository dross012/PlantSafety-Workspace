// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PermitToWork
//  Layer:          Infrastructure
//  File:           PermitToWorkDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PermitToWork aggregate. Implements both the
//                  Domain-layer write repository (IPermitToWorkRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetPermitToWorkItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  PermitToWork exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ContractorSafety.PermitToWork.Specifications;
using PlantSafety.Domain.ContractorSafety.PermitToWork.ValueObjects.Ids;
using PlantSafety.Application.ContractorSafety.PermitToWork.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.ContractorSafety.PermitToWork.Aggregates.PermitToWork;
using IWriteRepo = PlantSafety.Domain.ContractorSafety.PermitToWork.Interfaces.IPermitToWorkRepository;
using IReadRepo = PlantSafety.Application.ContractorSafety.PermitToWork.Repositories.IPermitToWorkRepository;

namespace PlantSafety.Infrastructure.ContractorSafety.PermitToWork.Persistence;

/// <summary>Flat Dapper row for [ContractorSafety].[PermitToWork], column-for-column with the PermitToWork aggregate.</summary>
internal sealed class PermitToWorkRow
{
    public Guid PermitToWorkId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? PermitTitle { get; init; }
    public string? PermitType { get; init; }
    public string? WorkLocation { get; init; }
    public int HazardCount { get; init; }
    public bool RequiresAtmosphericTesting { get; init; }
    public decimal? AtmosphericTestScore { get; init; }
    public Guid AuthorizedByUserId { get; init; }
    public DateTime? IssuedAtUtc { get; init; }
    public DateTime? ExpiresAtUtc { get; init; }
    public string? SuspensionReason { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the PermitToWork aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class PermitToWorkDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PermitToWorkDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "ContractorSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(PermitToWorkId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PermitToWork {PermitToWorkId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PermitToWorkId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<PermitToWorkRow>(
            $"[{Schema}].[usp_PermitToWork_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, PermitToWorkRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(PermitToWorkSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for PermitToWork");

        var rows = await queryExecutor.QueryAsync<PermitToWorkRow>(
            $"[{Schema}].[usp_PermitToWork_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, PermitToWorkRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for PermitToWork {PermitToWorkId}", aggregate.PermitToWorkId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("PermitToWorkId", aggregate, "PermitToWorkId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("PermitTitle", aggregate, "PermitTitle")
            .AddAggregateProperty("PermitType", aggregate, "PermitType")
            .AddAggregateProperty("WorkLocation", aggregate, "WorkLocation")
            .AddAggregateProperty("HazardCount", aggregate, "HazardCount")
            .AddAggregateProperty("RequiresAtmosphericTesting", aggregate, "RequiresAtmosphericTesting")
            .AddAggregateProperty("AtmosphericTestScore", aggregate, "AtmosphericTestScore")
            .AddAggregateProperty("AuthorizedByUserId", aggregate, "AuthorizedByUserId")
            .AddAggregateProperty("IssuedAtUtc", aggregate, "IssuedAtUtc")
            .AddAggregateProperty("ExpiresAtUtc", aggregate, "ExpiresAtUtc")
            .AddAggregateProperty("SuspensionReason", aggregate, "SuspensionReason")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_PermitToWork_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for PermitToWork {PermitToWorkId}", aggregate.PermitToWorkId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("PermitToWorkId", aggregate, "PermitToWorkId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("PermitTitle", aggregate, "PermitTitle")
            .AddAggregateProperty("PermitType", aggregate, "PermitType")
            .AddAggregateProperty("WorkLocation", aggregate, "WorkLocation")
            .AddAggregateProperty("HazardCount", aggregate, "HazardCount")
            .AddAggregateProperty("RequiresAtmosphericTesting", aggregate, "RequiresAtmosphericTesting")
            .AddAggregateProperty("AtmosphericTestScore", aggregate, "AtmosphericTestScore")
            .AddAggregateProperty("AuthorizedByUserId", aggregate, "AuthorizedByUserId")
            .AddAggregateProperty("IssuedAtUtc", aggregate, "IssuedAtUtc")
            .AddAggregateProperty("ExpiresAtUtc", aggregate, "ExpiresAtUtc")
            .AddAggregateProperty("SuspensionReason", aggregate, "SuspensionReason")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_PermitToWork_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetPermitToWorkItemsAsync"/>
    public async Task<IReadOnlyList<PermitToWorkItemDto>> GetPermitToWorkItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetPermitToWorkItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PermitToWorkItemDto>(
            $"[{Schema}].[usp_PermitToWork_GetItems]", parameters, cancellationToken);
    }
}
