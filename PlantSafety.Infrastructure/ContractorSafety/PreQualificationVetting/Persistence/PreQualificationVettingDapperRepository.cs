// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PreQualificationVetting
//  Layer:          Infrastructure
//  File:           PreQualificationVettingDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PreQualificationVetting aggregate. Implements both the
//                  Domain-layer write repository (IPreQualificationVettingRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetPreQualificationVettingItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  PreQualificationVetting exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Specifications;
using PlantSafety.Domain.ContractorSafety.PreQualificationVetting.ValueObjects.Ids;
using PlantSafety.Application.ContractorSafety.PreQualificationVetting.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Aggregates.PreQualificationVetting;
using IWriteRepo = PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Interfaces.IPreQualificationVettingRepository;
using IReadRepo = PlantSafety.Application.ContractorSafety.PreQualificationVetting.Repositories.IPreQualificationVettingRepository;

namespace PlantSafety.Infrastructure.ContractorSafety.PreQualificationVetting.Persistence;

/// <summary>Flat Dapper row for [ContractorSafety].[PreQualificationVetting], column-for-column with the PreQualificationVetting aggregate.</summary>
internal sealed class PreQualificationVettingRow
{
    public Guid PreQualificationVettingId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? VettingTitle { get; init; }
    public string? ContractorName { get; init; }
    public decimal? EmrScore { get; init; }
    public decimal? TrirScore { get; init; }
    public bool OshaTrainingCompliant { get; init; }
    public int DeficiencyCount { get; init; }
    public DateTime? InsuranceExpiresAtUtc { get; init; }
    public Guid ReviewedByUserId { get; init; }
    public DateTime? VettedAtUtc { get; init; }
    public DateTime? ExpiresAtUtc { get; init; }
    public string? RejectionReason { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the PreQualificationVetting aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class PreQualificationVettingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PreQualificationVettingDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "ContractorSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(PreQualificationVettingId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PreQualificationVetting {PreQualificationVettingId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PreQualificationVettingId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<PreQualificationVettingRow>(
            $"[{Schema}].[usp_PreQualificationVetting_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, PreQualificationVettingRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(PreQualificationVettingSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for PreQualificationVetting");

        var rows = await queryExecutor.QueryAsync<PreQualificationVettingRow>(
            $"[{Schema}].[usp_PreQualificationVetting_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, PreQualificationVettingRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for PreQualificationVetting {PreQualificationVettingId}", aggregate.PreQualificationVettingId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("PreQualificationVettingId", aggregate, "PreQualificationVettingId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("VettingTitle", aggregate, "VettingTitle")
            .AddAggregateProperty("ContractorName", aggregate, "ContractorName")
            .AddAggregateProperty("EmrScore", aggregate, "EmrScore")
            .AddAggregateProperty("TrirScore", aggregate, "TrirScore")
            .AddAggregateProperty("OshaTrainingCompliant", aggregate, "OshaTrainingCompliant")
            .AddAggregateProperty("DeficiencyCount", aggregate, "DeficiencyCount")
            .AddAggregateProperty("InsuranceExpiresAtUtc", aggregate, "InsuranceExpiresAtUtc")
            .AddAggregateProperty("ReviewedByUserId", aggregate, "ReviewedByUserId")
            .AddAggregateProperty("VettedAtUtc", aggregate, "VettedAtUtc")
            .AddAggregateProperty("ExpiresAtUtc", aggregate, "ExpiresAtUtc")
            .AddAggregateProperty("RejectionReason", aggregate, "RejectionReason")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_PreQualificationVetting_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for PreQualificationVetting {PreQualificationVettingId}", aggregate.PreQualificationVettingId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("PreQualificationVettingId", aggregate, "PreQualificationVettingId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("VettingTitle", aggregate, "VettingTitle")
            .AddAggregateProperty("ContractorName", aggregate, "ContractorName")
            .AddAggregateProperty("EmrScore", aggregate, "EmrScore")
            .AddAggregateProperty("TrirScore", aggregate, "TrirScore")
            .AddAggregateProperty("OshaTrainingCompliant", aggregate, "OshaTrainingCompliant")
            .AddAggregateProperty("DeficiencyCount", aggregate, "DeficiencyCount")
            .AddAggregateProperty("InsuranceExpiresAtUtc", aggregate, "InsuranceExpiresAtUtc")
            .AddAggregateProperty("ReviewedByUserId", aggregate, "ReviewedByUserId")
            .AddAggregateProperty("VettedAtUtc", aggregate, "VettedAtUtc")
            .AddAggregateProperty("ExpiresAtUtc", aggregate, "ExpiresAtUtc")
            .AddAggregateProperty("RejectionReason", aggregate, "RejectionReason")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_PreQualificationVetting_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetPreQualificationVettingItemsAsync"/>
    public async Task<IReadOnlyList<PreQualificationVettingItemDto>> GetPreQualificationVettingItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetPreQualificationVettingItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PreQualificationVettingItemDto>(
            $"[{Schema}].[usp_PreQualificationVetting_GetItems]", parameters, cancellationToken);
    }
}
