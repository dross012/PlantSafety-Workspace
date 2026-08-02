// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MaterialHandlingStorage
//  Layer:          Infrastructure
//  File:           MaterialHandlingStorageDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the MaterialHandlingStorage aggregate. Implements both the
//                  Domain-layer write repository (IMaterialHandlingStorageRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetMaterialHandlingStorageItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  MaterialHandlingStorage exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Specifications;
using PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.ValueObjects.Ids;
using PlantSafety.Application.SafetyProcedures.MaterialHandlingStorage.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Aggregates.MaterialHandlingStorage;
using IWriteRepo = PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Interfaces.IMaterialHandlingStorageRepository;
using IReadRepo = PlantSafety.Application.SafetyProcedures.MaterialHandlingStorage.Repositories.IMaterialHandlingStorageRepository;

namespace PlantSafety.Infrastructure.SafetyProcedures.MaterialHandlingStorage.Persistence;

/// <summary>Flat Dapper row for [SafetyProcedures].[MaterialHandlingStorage], column-for-column with the MaterialHandlingStorage aggregate.</summary>
internal sealed class MaterialHandlingStorageRow
{
    public Guid MaterialHandlingStorageId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? ProcedureTitle { get; init; }
    public string? HandlingCategory { get; init; }
    public string? StorageLocation { get; init; }
    public Guid ApprovedByUserId { get; init; }
    public DateTime? PublishedAtUtc { get; init; }
    public DateTime? NextReviewDueAtUtc { get; init; }
    public int ObstructionCount { get; init; }
    public decimal? ComplianceScore { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the MaterialHandlingStorage aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class MaterialHandlingStorageDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<MaterialHandlingStorageDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "SafetyProcedures";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(MaterialHandlingStorageId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for MaterialHandlingStorage {MaterialHandlingStorageId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("MaterialHandlingStorageId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<MaterialHandlingStorageRow>(
            $"[{Schema}].[usp_MaterialHandlingStorage_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, MaterialHandlingStorageRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(MaterialHandlingStorageSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for MaterialHandlingStorage");

        var rows = await queryExecutor.QueryAsync<MaterialHandlingStorageRow>(
            $"[{Schema}].[usp_MaterialHandlingStorage_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, MaterialHandlingStorageRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for MaterialHandlingStorage {MaterialHandlingStorageId}", aggregate.MaterialHandlingStorageId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("MaterialHandlingStorageId", aggregate, "MaterialHandlingStorageId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ProcedureTitle", aggregate, "ProcedureTitle")
            .AddAggregateProperty("HandlingCategory", aggregate, "HandlingCategory")
            .AddAggregateProperty("StorageLocation", aggregate, "StorageLocation")
            .AddAggregateProperty("ApprovedByUserId", aggregate, "ApprovedByUserId")
            .AddAggregateProperty("PublishedAtUtc", aggregate, "PublishedAtUtc")
            .AddAggregateProperty("NextReviewDueAtUtc", aggregate, "NextReviewDueAtUtc")
            .AddAggregateProperty("ObstructionCount", aggregate, "ObstructionCount")
            .AddAggregateProperty("ComplianceScore", aggregate, "ComplianceScore")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_MaterialHandlingStorage_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for MaterialHandlingStorage {MaterialHandlingStorageId}", aggregate.MaterialHandlingStorageId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("MaterialHandlingStorageId", aggregate, "MaterialHandlingStorageId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ProcedureTitle", aggregate, "ProcedureTitle")
            .AddAggregateProperty("HandlingCategory", aggregate, "HandlingCategory")
            .AddAggregateProperty("StorageLocation", aggregate, "StorageLocation")
            .AddAggregateProperty("ApprovedByUserId", aggregate, "ApprovedByUserId")
            .AddAggregateProperty("PublishedAtUtc", aggregate, "PublishedAtUtc")
            .AddAggregateProperty("NextReviewDueAtUtc", aggregate, "NextReviewDueAtUtc")
            .AddAggregateProperty("ObstructionCount", aggregate, "ObstructionCount")
            .AddAggregateProperty("ComplianceScore", aggregate, "ComplianceScore")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_MaterialHandlingStorage_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetMaterialHandlingStorageItemsAsync"/>
    public async Task<IReadOnlyList<MaterialHandlingStorageItemDto>> GetMaterialHandlingStorageItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetMaterialHandlingStorageItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<MaterialHandlingStorageItemDto>(
            $"[{Schema}].[usp_MaterialHandlingStorage_GetItems]", parameters, cancellationToken);
    }
}
