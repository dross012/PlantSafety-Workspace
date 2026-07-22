// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.MachineGuarding
//  Layer:          Infrastructure
//  File:           MachineGuardingDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the MachineGuarding aggregate. Implements both the
//                  Domain-layer write repository (IMachineGuardingRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetMachineGuardingItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  MachineGuarding exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.EmployeeSafety.MachineGuarding.Specifications;
using PlantSafety.Domain.EmployeeSafety.MachineGuarding.ValueObjects.Ids;
using PlantSafety.Application.EmployeeSafety.MachineGuarding.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.EmployeeSafety.MachineGuarding.Aggregates.MachineGuarding;
using IWriteRepo = PlantSafety.Domain.EmployeeSafety.MachineGuarding.Interfaces.IMachineGuardingRepository;
using IReadRepo = PlantSafety.Application.EmployeeSafety.MachineGuarding.Repositories.IMachineGuardingRepository;

namespace PlantSafety.Infrastructure.EmployeeSafety.MachineGuarding.Persistence;

/// <summary>Flat Dapper row for [EmployeeSafety].[MachineGuarding], column-for-column with the MachineGuarding aggregate.</summary>
internal sealed class MachineGuardingRow
{
    public Guid MachineGuardingId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? GuardTitle { get; init; }
    public string? GuardType { get; init; }
    public string? EquipmentTag { get; init; }
    public string? Location { get; init; }
    public bool HasInterlock { get; init; }
    public int InspectionIntervalDays { get; init; }
    public Guid? LastInspectedByUserId { get; init; }
    public DateTime? LastInspectedAtUtc { get; init; }
    public DateTime? NextInspectionDueAtUtc { get; init; }
    public int OpenDeficiencyCount { get; init; }
    public decimal? GuardConditionScore { get; init; }
    public bool BypassDetected { get; init; }
    public string? LastFindingNotes { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the MachineGuarding aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class MachineGuardingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<MachineGuardingDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "EmployeeSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(MachineGuardingId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for MachineGuarding {MachineGuardingId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("MachineGuardingId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<MachineGuardingRow>(
            $"[{Schema}].[usp_MachineGuarding_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, MachineGuardingRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(MachineGuardingSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for MachineGuarding");

        var rows = await queryExecutor.QueryAsync<MachineGuardingRow>(
            $"[{Schema}].[usp_MachineGuarding_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, MachineGuardingRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for MachineGuarding {MachineGuardingId}", aggregate.MachineGuardingId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("MachineGuardingId", aggregate, "MachineGuardingId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("GuardTitle", aggregate, "GuardTitle")
            .AddAggregateProperty("GuardType", aggregate, "GuardType")
            .AddAggregateProperty("EquipmentTag", aggregate, "EquipmentTag")
            .AddAggregateProperty("Location", aggregate, "Location")
            .AddAggregateProperty("HasInterlock", aggregate, "HasInterlock")
            .AddAggregateProperty("InspectionIntervalDays", aggregate, "InspectionIntervalDays")
            .AddAggregateProperty("LastInspectedByUserId", aggregate, "LastInspectedByUserId")
            .AddAggregateProperty("LastInspectedAtUtc", aggregate, "LastInspectedAtUtc")
            .AddAggregateProperty("NextInspectionDueAtUtc", aggregate, "NextInspectionDueAtUtc")
            .AddAggregateProperty("OpenDeficiencyCount", aggregate, "OpenDeficiencyCount")
            .AddAggregateProperty("GuardConditionScore", aggregate, "GuardConditionScore")
            .AddAggregateProperty("BypassDetected", aggregate, "BypassDetected")
            .AddAggregateProperty("LastFindingNotes", aggregate, "LastFindingNotes")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_MachineGuarding_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for MachineGuarding {MachineGuardingId}", aggregate.MachineGuardingId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("MachineGuardingId", aggregate, "MachineGuardingId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("GuardTitle", aggregate, "GuardTitle")
            .AddAggregateProperty("GuardType", aggregate, "GuardType")
            .AddAggregateProperty("EquipmentTag", aggregate, "EquipmentTag")
            .AddAggregateProperty("Location", aggregate, "Location")
            .AddAggregateProperty("HasInterlock", aggregate, "HasInterlock")
            .AddAggregateProperty("InspectionIntervalDays", aggregate, "InspectionIntervalDays")
            .AddAggregateProperty("LastInspectedByUserId", aggregate, "LastInspectedByUserId")
            .AddAggregateProperty("LastInspectedAtUtc", aggregate, "LastInspectedAtUtc")
            .AddAggregateProperty("NextInspectionDueAtUtc", aggregate, "NextInspectionDueAtUtc")
            .AddAggregateProperty("OpenDeficiencyCount", aggregate, "OpenDeficiencyCount")
            .AddAggregateProperty("GuardConditionScore", aggregate, "GuardConditionScore")
            .AddAggregateProperty("BypassDetected", aggregate, "BypassDetected")
            .AddAggregateProperty("LastFindingNotes", aggregate, "LastFindingNotes")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_MachineGuarding_Update]", parameters, cancellationToken: cancellationToken);
    }

    /// <inheritdoc cref="IReadRepo.GetMachineGuardingItemsAsync"/>
    public async Task<IReadOnlyList<MachineGuardingItemDto>> GetMachineGuardingItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetMachineGuardingItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<MachineGuardingItemDto>(
            $"[{Schema}].[usp_MachineGuarding_GetItems]", parameters, cancellationToken);
    }
}
