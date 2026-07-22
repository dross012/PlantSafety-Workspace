// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.LotoPrograms
//  Layer:          Infrastructure
//  File:           LotoProgramsDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the LotoPrograms aggregate. Implements both the
//                  Domain-layer write repository (ILotoProgramsRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetLotoProgramsItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  LotoPrograms exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.EmployeeSafety.LotoPrograms.Specifications;
using PlantSafety.Domain.EmployeeSafety.LotoPrograms.ValueObjects.Ids;
using PlantSafety.Application.EmployeeSafety.LotoPrograms.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.EmployeeSafety.LotoPrograms.Aggregates.LotoPrograms;
using IWriteRepo = PlantSafety.Domain.EmployeeSafety.LotoPrograms.Interfaces.ILotoProgramsRepository;
using IReadRepo = PlantSafety.Application.EmployeeSafety.LotoPrograms.Repositories.ILotoProgramsRepository;

namespace PlantSafety.Infrastructure.EmployeeSafety.LotoPrograms.Persistence;

/// <summary>Flat Dapper row for [EmployeeSafety].[LotoPrograms], column-for-column with the LotoPrograms aggregate.</summary>
internal sealed class LotoProgramsRow
{
    public Guid LotoProgramsId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? ProgramTitle { get; init; }
    public string? EquipmentTag { get; init; }
    public string? EnergySourceTypes { get; init; }
    public string? IsolationPointDescription { get; init; }
    public int IsolationPointCount { get; init; }
    public Guid AuthorizedByUserId { get; init; }
    public int LocksAppliedCount { get; init; }
    public Guid? VerifiedByUserId { get; init; }
    public DateTime? VerifiedZeroEnergyAtUtc { get; init; }
    public Guid? ReleasedByUserId { get; init; }
    public DateTime? ReleasedAtUtc { get; init; }
    public DateTime? LastAuditedAtUtc { get; init; }
    public decimal? ProgramComplianceScore { get; init; }
    public string? NonComplianceNotes { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the LotoPrograms aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class LotoProgramsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<LotoProgramsDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "EmployeeSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(LotoProgramsId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for LotoPrograms {LotoProgramsId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("LotoProgramsId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<LotoProgramsRow>(
            $"[{Schema}].[usp_LotoPrograms_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, LotoProgramsRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(LotoProgramsSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for LotoPrograms");

        var rows = await queryExecutor.QueryAsync<LotoProgramsRow>(
            $"[{Schema}].[usp_LotoPrograms_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, LotoProgramsRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for LotoPrograms {LotoProgramsId}", aggregate.LotoProgramsId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("LotoProgramsId", aggregate, "LotoProgramsId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ProgramTitle", aggregate, "ProgramTitle")
            .AddAggregateProperty("EquipmentTag", aggregate, "EquipmentTag")
            .AddAggregateProperty("EnergySourceTypes", aggregate, "EnergySourceTypes")
            .AddAggregateProperty("IsolationPointDescription", aggregate, "IsolationPointDescription")
            .AddAggregateProperty("IsolationPointCount", aggregate, "IsolationPointCount")
            .AddAggregateProperty("AuthorizedByUserId", aggregate, "AuthorizedByUserId")
            .AddAggregateProperty("LocksAppliedCount", aggregate, "LocksAppliedCount")
            .AddAggregateProperty("VerifiedByUserId", aggregate, "VerifiedByUserId")
            .AddAggregateProperty("VerifiedZeroEnergyAtUtc", aggregate, "VerifiedZeroEnergyAtUtc")
            .AddAggregateProperty("ReleasedByUserId", aggregate, "ReleasedByUserId")
            .AddAggregateProperty("ReleasedAtUtc", aggregate, "ReleasedAtUtc")
            .AddAggregateProperty("LastAuditedAtUtc", aggregate, "LastAuditedAtUtc")
            .AddAggregateProperty("ProgramComplianceScore", aggregate, "ProgramComplianceScore")
            .AddAggregateProperty("NonComplianceNotes", aggregate, "NonComplianceNotes")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_LotoPrograms_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for LotoPrograms {LotoProgramsId}", aggregate.LotoProgramsId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("LotoProgramsId", aggregate, "LotoProgramsId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ProgramTitle", aggregate, "ProgramTitle")
            .AddAggregateProperty("EquipmentTag", aggregate, "EquipmentTag")
            .AddAggregateProperty("EnergySourceTypes", aggregate, "EnergySourceTypes")
            .AddAggregateProperty("IsolationPointDescription", aggregate, "IsolationPointDescription")
            .AddAggregateProperty("IsolationPointCount", aggregate, "IsolationPointCount")
            .AddAggregateProperty("AuthorizedByUserId", aggregate, "AuthorizedByUserId")
            .AddAggregateProperty("LocksAppliedCount", aggregate, "LocksAppliedCount")
            .AddAggregateProperty("VerifiedByUserId", aggregate, "VerifiedByUserId")
            .AddAggregateProperty("VerifiedZeroEnergyAtUtc", aggregate, "VerifiedZeroEnergyAtUtc")
            .AddAggregateProperty("ReleasedByUserId", aggregate, "ReleasedByUserId")
            .AddAggregateProperty("ReleasedAtUtc", aggregate, "ReleasedAtUtc")
            .AddAggregateProperty("LastAuditedAtUtc", aggregate, "LastAuditedAtUtc")
            .AddAggregateProperty("ProgramComplianceScore", aggregate, "ProgramComplianceScore")
            .AddAggregateProperty("NonComplianceNotes", aggregate, "NonComplianceNotes")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_LotoPrograms_Update]", parameters, cancellationToken: cancellationToken);
    }

    /// <inheritdoc cref="IReadRepo.GetLotoProgramsItemsAsync"/>
    public async Task<IReadOnlyList<LotoProgramsItemDto>> GetLotoProgramsItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetLotoProgramsItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<LotoProgramsItemDto>(
            $"[{Schema}].[usp_LotoPrograms_GetItems]", parameters, cancellationToken);
    }
}
