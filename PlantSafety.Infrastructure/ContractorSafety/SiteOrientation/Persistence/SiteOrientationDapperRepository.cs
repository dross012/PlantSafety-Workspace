// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.SiteOrientation
//  Layer:          Infrastructure
//  File:           SiteOrientationDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the SiteOrientation aggregate. Implements both the
//                  Domain-layer write repository (ISiteOrientationRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetSiteOrientationItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  SiteOrientation exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ContractorSafety.SiteOrientation.Specifications;
using PlantSafety.Domain.ContractorSafety.SiteOrientation.ValueObjects.Ids;
using PlantSafety.Application.ContractorSafety.SiteOrientation.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.ContractorSafety.SiteOrientation.Aggregates.SiteOrientation;
using IWriteRepo = PlantSafety.Domain.ContractorSafety.SiteOrientation.Interfaces.ISiteOrientationRepository;
using IReadRepo = PlantSafety.Application.ContractorSafety.SiteOrientation.Repositories.ISiteOrientationRepository;

namespace PlantSafety.Infrastructure.ContractorSafety.SiteOrientation.Persistence;

/// <summary>Flat Dapper row for [ContractorSafety].[SiteOrientation], column-for-column with the SiteOrientation aggregate.</summary>
internal sealed class SiteOrientationRow
{
    public Guid SiteOrientationId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? OrientationTitle { get; init; }
    public string? ContractorName { get; init; }
    public Guid ConductedByUserId { get; init; }
    public DateTime? DeliveredAtUtc { get; init; }
    public int AttendeeCount { get; init; }
    public decimal? ComprehensionScore { get; init; }
    public decimal PassingScoreThreshold { get; init; }
    public DateTime? ExpiresAtUtc { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the SiteOrientation aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class SiteOrientationDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<SiteOrientationDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "ContractorSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(SiteOrientationId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for SiteOrientation {SiteOrientationId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("SiteOrientationId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<SiteOrientationRow>(
            $"[{Schema}].[usp_SiteOrientation_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, SiteOrientationRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(SiteOrientationSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for SiteOrientation");

        var rows = await queryExecutor.QueryAsync<SiteOrientationRow>(
            $"[{Schema}].[usp_SiteOrientation_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, SiteOrientationRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for SiteOrientation {SiteOrientationId}", aggregate.SiteOrientationId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("SiteOrientationId", aggregate, "SiteOrientationId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("OrientationTitle", aggregate, "OrientationTitle")
            .AddAggregateProperty("ContractorName", aggregate, "ContractorName")
            .AddAggregateProperty("ConductedByUserId", aggregate, "ConductedByUserId")
            .AddAggregateProperty("DeliveredAtUtc", aggregate, "DeliveredAtUtc")
            .AddAggregateProperty("AttendeeCount", aggregate, "AttendeeCount")
            .AddAggregateProperty("ComprehensionScore", aggregate, "ComprehensionScore")
            .AddAggregateProperty("PassingScoreThreshold", aggregate, "PassingScoreThreshold")
            .AddAggregateProperty("ExpiresAtUtc", aggregate, "ExpiresAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_SiteOrientation_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for SiteOrientation {SiteOrientationId}", aggregate.SiteOrientationId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("SiteOrientationId", aggregate, "SiteOrientationId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("OrientationTitle", aggregate, "OrientationTitle")
            .AddAggregateProperty("ContractorName", aggregate, "ContractorName")
            .AddAggregateProperty("ConductedByUserId", aggregate, "ConductedByUserId")
            .AddAggregateProperty("DeliveredAtUtc", aggregate, "DeliveredAtUtc")
            .AddAggregateProperty("AttendeeCount", aggregate, "AttendeeCount")
            .AddAggregateProperty("ComprehensionScore", aggregate, "ComprehensionScore")
            .AddAggregateProperty("PassingScoreThreshold", aggregate, "PassingScoreThreshold")
            .AddAggregateProperty("ExpiresAtUtc", aggregate, "ExpiresAtUtc")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_SiteOrientation_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetSiteOrientationItemsAsync"/>
    public async Task<IReadOnlyList<SiteOrientationItemDto>> GetSiteOrientationItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetSiteOrientationItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<SiteOrientationItemDto>(
            $"[{Schema}].[usp_SiteOrientation_GetItems]", parameters, cancellationToken);
    }
}
