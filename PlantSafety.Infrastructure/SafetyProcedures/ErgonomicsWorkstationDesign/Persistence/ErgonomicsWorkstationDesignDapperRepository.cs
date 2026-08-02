// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.ErgonomicsWorkstationDesign
//  Layer:          Infrastructure
//  File:           ErgonomicsWorkstationDesignDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ErgonomicsWorkstationDesign aggregate. Implements both the
//                  Domain-layer write repository (IErgonomicsWorkstationDesignRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetErgonomicsWorkstationDesignItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  ErgonomicsWorkstationDesign exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Specifications;
using PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.ValueObjects.Ids;
using PlantSafety.Application.SafetyProcedures.ErgonomicsWorkstationDesign.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Aggregates.ErgonomicsWorkstationDesign;
using IWriteRepo = PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Interfaces.IErgonomicsWorkstationDesignRepository;
using IReadRepo = PlantSafety.Application.SafetyProcedures.ErgonomicsWorkstationDesign.Repositories.IErgonomicsWorkstationDesignRepository;

namespace PlantSafety.Infrastructure.SafetyProcedures.ErgonomicsWorkstationDesign.Persistence;

/// <summary>Flat Dapper row for [SafetyProcedures].[ErgonomicsWorkstationDesign], column-for-column with the ErgonomicsWorkstationDesign aggregate.</summary>
internal sealed class ErgonomicsWorkstationDesignRow
{
    public Guid ErgonomicsWorkstationDesignId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? ProcedureTitle { get; init; }
    public string? DesignCategory { get; init; }
    public string? WorkstationLocation { get; init; }
    public Guid ApprovedByUserId { get; init; }
    public DateTime? AssessedAtUtc { get; init; }
    public DateTime? NextAssessmentDueAtUtc { get; init; }
    public int RiskFactorCount { get; init; }
    public decimal? ErgonomicsScore { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the ErgonomicsWorkstationDesign aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class ErgonomicsWorkstationDesignDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<ErgonomicsWorkstationDesignDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "SafetyProcedures";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(ErgonomicsWorkstationDesignId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for ErgonomicsWorkstationDesign {ErgonomicsWorkstationDesignId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ErgonomicsWorkstationDesignId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<ErgonomicsWorkstationDesignRow>(
            $"[{Schema}].[usp_ErgonomicsWorkstationDesign_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, ErgonomicsWorkstationDesignRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(ErgonomicsWorkstationDesignSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for ErgonomicsWorkstationDesign");

        var rows = await queryExecutor.QueryAsync<ErgonomicsWorkstationDesignRow>(
            $"[{Schema}].[usp_ErgonomicsWorkstationDesign_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, ErgonomicsWorkstationDesignRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for ErgonomicsWorkstationDesign {ErgonomicsWorkstationDesignId}", aggregate.ErgonomicsWorkstationDesignId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("ErgonomicsWorkstationDesignId", aggregate, "ErgonomicsWorkstationDesignId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ProcedureTitle", aggregate, "ProcedureTitle")
            .AddAggregateProperty("DesignCategory", aggregate, "DesignCategory")
            .AddAggregateProperty("WorkstationLocation", aggregate, "WorkstationLocation")
            .AddAggregateProperty("ApprovedByUserId", aggregate, "ApprovedByUserId")
            .AddAggregateProperty("AssessedAtUtc", aggregate, "AssessedAtUtc")
            .AddAggregateProperty("NextAssessmentDueAtUtc", aggregate, "NextAssessmentDueAtUtc")
            .AddAggregateProperty("RiskFactorCount", aggregate, "RiskFactorCount")
            .AddAggregateProperty("ErgonomicsScore", aggregate, "ErgonomicsScore")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_ErgonomicsWorkstationDesign_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for ErgonomicsWorkstationDesign {ErgonomicsWorkstationDesignId}", aggregate.ErgonomicsWorkstationDesignId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("ErgonomicsWorkstationDesignId", aggregate, "ErgonomicsWorkstationDesignId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ProcedureTitle", aggregate, "ProcedureTitle")
            .AddAggregateProperty("DesignCategory", aggregate, "DesignCategory")
            .AddAggregateProperty("WorkstationLocation", aggregate, "WorkstationLocation")
            .AddAggregateProperty("ApprovedByUserId", aggregate, "ApprovedByUserId")
            .AddAggregateProperty("AssessedAtUtc", aggregate, "AssessedAtUtc")
            .AddAggregateProperty("NextAssessmentDueAtUtc", aggregate, "NextAssessmentDueAtUtc")
            .AddAggregateProperty("RiskFactorCount", aggregate, "RiskFactorCount")
            .AddAggregateProperty("ErgonomicsScore", aggregate, "ErgonomicsScore")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_ErgonomicsWorkstationDesign_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetErgonomicsWorkstationDesignItemsAsync"/>
    public async Task<IReadOnlyList<ErgonomicsWorkstationDesignItemDto>> GetErgonomicsWorkstationDesignItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetErgonomicsWorkstationDesignItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ErgonomicsWorkstationDesignItemDto>(
            $"[{Schema}].[usp_ErgonomicsWorkstationDesign_GetItems]", parameters, cancellationToken);
    }
}
