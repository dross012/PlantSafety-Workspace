// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.ErgonomicsAndMaterialHandling
//  Layer:          Infrastructure
//  File:           ErgonomicsAndMaterialHandlingDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ErgonomicsAndMaterialHandling aggregate. Implements both the
//                  Domain-layer write repository (IErgonomicsAndMaterialHandlingRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetErgonomicsAndMaterialHandlingItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  ErgonomicsAndMaterialHandling exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Specifications;
using PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.ValueObjects.Ids;
using PlantSafety.Application.EmployeeSafety.ErgonomicsAndMaterialHandling.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Aggregates.ErgonomicsAndMaterialHandling;
using IWriteRepo = PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Interfaces.IErgonomicsAndMaterialHandlingRepository;
using IReadRepo = PlantSafety.Application.EmployeeSafety.ErgonomicsAndMaterialHandling.Repositories.IErgonomicsAndMaterialHandlingRepository;

namespace PlantSafety.Infrastructure.EmployeeSafety.ErgonomicsAndMaterialHandling.Persistence;

/// <summary>Flat Dapper row for [EmployeeSafety].[ErgonomicsAndMaterialHandling], column-for-column with the ErgonomicsAndMaterialHandling aggregate.</summary>
internal sealed class ErgonomicsAndMaterialHandlingRow
{
    public Guid ErgonomicsAndMaterialHandlingId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? AssessmentTitle { get; init; }
    public string? HazardCategory { get; init; }
    public string? WorkstationLocation { get; init; }
    public Guid AssessedByUserId { get; init; }
    public DateTime? AssessedAtUtc { get; init; }
    public int RiskFactorCount { get; init; }
    public decimal? RiskScore { get; init; }
    public string? ControlsDescription { get; init; }
    public DateTime? FollowUpDueAtUtc { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the ErgonomicsAndMaterialHandling aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class ErgonomicsAndMaterialHandlingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<ErgonomicsAndMaterialHandlingDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "EmployeeSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(ErgonomicsAndMaterialHandlingId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for ErgonomicsAndMaterialHandling {ErgonomicsAndMaterialHandlingId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ErgonomicsAndMaterialHandlingId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<ErgonomicsAndMaterialHandlingRow>(
            $"[{Schema}].[usp_ErgonomicsAndMaterialHandling_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, ErgonomicsAndMaterialHandlingRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(ErgonomicsAndMaterialHandlingSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for ErgonomicsAndMaterialHandling");

        var rows = await queryExecutor.QueryAsync<ErgonomicsAndMaterialHandlingRow>(
            $"[{Schema}].[usp_ErgonomicsAndMaterialHandling_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, ErgonomicsAndMaterialHandlingRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for ErgonomicsAndMaterialHandling {ErgonomicsAndMaterialHandlingId}", aggregate.ErgonomicsAndMaterialHandlingId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("ErgonomicsAndMaterialHandlingId", aggregate, "ErgonomicsAndMaterialHandlingId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AssessmentTitle", aggregate, "AssessmentTitle")
            .AddAggregateProperty("HazardCategory", aggregate, "HazardCategory")
            .AddAggregateProperty("WorkstationLocation", aggregate, "WorkstationLocation")
            .AddAggregateProperty("AssessedByUserId", aggregate, "AssessedByUserId")
            .AddAggregateProperty("AssessedAtUtc", aggregate, "AssessedAtUtc")
            .AddAggregateProperty("RiskFactorCount", aggregate, "RiskFactorCount")
            .AddAggregateProperty("RiskScore", aggregate, "RiskScore")
            .AddAggregateProperty("ControlsDescription", aggregate, "ControlsDescription")
            .AddAggregateProperty("FollowUpDueAtUtc", aggregate, "FollowUpDueAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_ErgonomicsAndMaterialHandling_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for ErgonomicsAndMaterialHandling {ErgonomicsAndMaterialHandlingId}", aggregate.ErgonomicsAndMaterialHandlingId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("ErgonomicsAndMaterialHandlingId", aggregate, "ErgonomicsAndMaterialHandlingId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AssessmentTitle", aggregate, "AssessmentTitle")
            .AddAggregateProperty("HazardCategory", aggregate, "HazardCategory")
            .AddAggregateProperty("WorkstationLocation", aggregate, "WorkstationLocation")
            .AddAggregateProperty("AssessedByUserId", aggregate, "AssessedByUserId")
            .AddAggregateProperty("AssessedAtUtc", aggregate, "AssessedAtUtc")
            .AddAggregateProperty("RiskFactorCount", aggregate, "RiskFactorCount")
            .AddAggregateProperty("RiskScore", aggregate, "RiskScore")
            .AddAggregateProperty("ControlsDescription", aggregate, "ControlsDescription")
            .AddAggregateProperty("FollowUpDueAtUtc", aggregate, "FollowUpDueAtUtc")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_ErgonomicsAndMaterialHandling_Update]", parameters, cancellationToken: cancellationToken);
    }

    /// <inheritdoc cref="IReadRepo.GetErgonomicsAndMaterialHandlingItemsAsync"/>
    public async Task<IReadOnlyList<ErgonomicsAndMaterialHandlingItemDto>> GetErgonomicsAndMaterialHandlingItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetErgonomicsAndMaterialHandlingItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ErgonomicsAndMaterialHandlingItemDto>(
            $"[{Schema}].[usp_ErgonomicsAndMaterialHandling_GetItems]", parameters, cancellationToken);
    }
}
