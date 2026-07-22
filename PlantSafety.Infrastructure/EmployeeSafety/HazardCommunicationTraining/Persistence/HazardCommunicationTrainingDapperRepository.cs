// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.HazardCommunicationTraining
//  Layer:          Infrastructure
//  File:           HazardCommunicationTrainingDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the HazardCommunicationTraining aggregate. Implements both the
//                  Domain-layer write repository (IHazardCommunicationTrainingRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetHazardCommunicationTrainingItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  HazardCommunicationTraining exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Specifications;
using PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.ValueObjects.Ids;
using PlantSafety.Application.EmployeeSafety.HazardCommunicationTraining.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Aggregates.HazardCommunicationTraining;
using IWriteRepo = PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Interfaces.IHazardCommunicationTrainingRepository;
using IReadRepo = PlantSafety.Application.EmployeeSafety.HazardCommunicationTraining.Repositories.IHazardCommunicationTrainingRepository;

namespace PlantSafety.Infrastructure.EmployeeSafety.HazardCommunicationTraining.Persistence;

/// <summary>Flat Dapper row for [EmployeeSafety].[HazardCommunicationTraining], column-for-column with the HazardCommunicationTraining aggregate.</summary>
internal sealed class HazardCommunicationTrainingRow
{
    public Guid HazardCommunicationTrainingId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? ProgramTitle { get; init; }
    public string? HazardClass { get; init; }
    public string? SdsReference { get; init; }
    public Guid TrainedByUserId { get; init; }
    public DateTime? TrainingDeliveredAtUtc { get; init; }
    public int EmployeesTrainedCount { get; init; }
    public decimal? ComprehensionScore { get; init; }
    public decimal PassingScoreThreshold { get; init; }
    public DateTime? NextTrainingDueAtUtc { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the HazardCommunicationTraining aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class HazardCommunicationTrainingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<HazardCommunicationTrainingDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "EmployeeSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(HazardCommunicationTrainingId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for HazardCommunicationTraining {HazardCommunicationTrainingId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("HazardCommunicationTrainingId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<HazardCommunicationTrainingRow>(
            $"[{Schema}].[usp_HazardCommunicationTraining_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, HazardCommunicationTrainingRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(HazardCommunicationTrainingSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for HazardCommunicationTraining");

        var rows = await queryExecutor.QueryAsync<HazardCommunicationTrainingRow>(
            $"[{Schema}].[usp_HazardCommunicationTraining_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, HazardCommunicationTrainingRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for HazardCommunicationTraining {HazardCommunicationTrainingId}", aggregate.HazardCommunicationTrainingId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("HazardCommunicationTrainingId", aggregate, "HazardCommunicationTrainingId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ProgramTitle", aggregate, "ProgramTitle")
            .AddAggregateProperty("HazardClass", aggregate, "HazardClass")
            .AddAggregateProperty("SdsReference", aggregate, "SdsReference")
            .AddAggregateProperty("TrainedByUserId", aggregate, "TrainedByUserId")
            .AddAggregateProperty("TrainingDeliveredAtUtc", aggregate, "TrainingDeliveredAtUtc")
            .AddAggregateProperty("EmployeesTrainedCount", aggregate, "EmployeesTrainedCount")
            .AddAggregateProperty("ComprehensionScore", aggregate, "ComprehensionScore")
            .AddAggregateProperty("PassingScoreThreshold", aggregate, "PassingScoreThreshold")
            .AddAggregateProperty("NextTrainingDueAtUtc", aggregate, "NextTrainingDueAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_HazardCommunicationTraining_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for HazardCommunicationTraining {HazardCommunicationTrainingId}", aggregate.HazardCommunicationTrainingId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("HazardCommunicationTrainingId", aggregate, "HazardCommunicationTrainingId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ProgramTitle", aggregate, "ProgramTitle")
            .AddAggregateProperty("HazardClass", aggregate, "HazardClass")
            .AddAggregateProperty("SdsReference", aggregate, "SdsReference")
            .AddAggregateProperty("TrainedByUserId", aggregate, "TrainedByUserId")
            .AddAggregateProperty("TrainingDeliveredAtUtc", aggregate, "TrainingDeliveredAtUtc")
            .AddAggregateProperty("EmployeesTrainedCount", aggregate, "EmployeesTrainedCount")
            .AddAggregateProperty("ComprehensionScore", aggregate, "ComprehensionScore")
            .AddAggregateProperty("PassingScoreThreshold", aggregate, "PassingScoreThreshold")
            .AddAggregateProperty("NextTrainingDueAtUtc", aggregate, "NextTrainingDueAtUtc")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_HazardCommunicationTraining_Update]", parameters, cancellationToken: cancellationToken);
    }

    /// <inheritdoc cref="IReadRepo.GetHazardCommunicationTrainingItemsAsync"/>
    public async Task<IReadOnlyList<HazardCommunicationTrainingItemDto>> GetHazardCommunicationTrainingItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetHazardCommunicationTrainingItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<HazardCommunicationTrainingItemDto>(
            $"[{Schema}].[usp_HazardCommunicationTraining_GetItems]", parameters, cancellationToken);
    }
}
