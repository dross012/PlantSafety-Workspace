// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.MachineGuardsBarriers
//  Layer:          Infrastructure
//  File:           MachineGuardsBarriersDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the MachineGuardsBarriers aggregate. Implements both the
//                  Domain-layer write repository (IMachineGuardsBarriersRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetMachineGuardsBarriersItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  MachineGuardsBarriers exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Specifications;
using PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.ValueObjects.Ids;
using PlantSafety.Application.MachineSafety.MachineGuardsBarriers.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Aggregates.MachineGuardsBarriers;
using IWriteRepo = PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Interfaces.IMachineGuardsBarriersRepository;
using IReadRepo = PlantSafety.Application.MachineSafety.MachineGuardsBarriers.Repositories.IMachineGuardsBarriersRepository;

namespace PlantSafety.Infrastructure.MachineSafety.MachineGuardsBarriers.Persistence;

/// <summary>Flat Dapper row for [MachineSafety].[MachineGuardsBarriers], column-for-column with the MachineGuardsBarriers aggregate.</summary>
internal sealed class MachineGuardsBarriersRow
{
    public Guid MachineGuardsBarriersId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? GuardTitle { get; init; }
    public string? GuardType { get; init; }
    public string? EquipmentLocation { get; init; }
    public Guid InstalledByUserId { get; init; }
    public DateTime? InspectedAtUtc { get; init; }
    public DateTime? NextInspectionDueAtUtc { get; init; }
    public int DeficiencyCount { get; init; }
    public decimal? GuardIntegrityScore { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the MachineGuardsBarriers aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class MachineGuardsBarriersDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<MachineGuardsBarriersDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "MachineSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(MachineGuardsBarriersId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for MachineGuardsBarriers {MachineGuardsBarriersId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("MachineGuardsBarriersId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<MachineGuardsBarriersRow>(
            $"[{Schema}].[usp_MachineGuardsBarriers_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, MachineGuardsBarriersRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(MachineGuardsBarriersSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for MachineGuardsBarriers");

        var rows = await queryExecutor.QueryAsync<MachineGuardsBarriersRow>(
            $"[{Schema}].[usp_MachineGuardsBarriers_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, MachineGuardsBarriersRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for MachineGuardsBarriers {MachineGuardsBarriersId}", aggregate.MachineGuardsBarriersId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("MachineGuardsBarriersId", aggregate, "MachineGuardsBarriersId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("GuardTitle", aggregate, "GuardTitle")
            .AddAggregateProperty("GuardType", aggregate, "GuardType")
            .AddAggregateProperty("EquipmentLocation", aggregate, "EquipmentLocation")
            .AddAggregateProperty("InstalledByUserId", aggregate, "InstalledByUserId")
            .AddAggregateProperty("InspectedAtUtc", aggregate, "InspectedAtUtc")
            .AddAggregateProperty("NextInspectionDueAtUtc", aggregate, "NextInspectionDueAtUtc")
            .AddAggregateProperty("DeficiencyCount", aggregate, "DeficiencyCount")
            .AddAggregateProperty("GuardIntegrityScore", aggregate, "GuardIntegrityScore")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_MachineGuardsBarriers_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for MachineGuardsBarriers {MachineGuardsBarriersId}", aggregate.MachineGuardsBarriersId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("MachineGuardsBarriersId", aggregate, "MachineGuardsBarriersId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("GuardTitle", aggregate, "GuardTitle")
            .AddAggregateProperty("GuardType", aggregate, "GuardType")
            .AddAggregateProperty("EquipmentLocation", aggregate, "EquipmentLocation")
            .AddAggregateProperty("InstalledByUserId", aggregate, "InstalledByUserId")
            .AddAggregateProperty("InspectedAtUtc", aggregate, "InspectedAtUtc")
            .AddAggregateProperty("NextInspectionDueAtUtc", aggregate, "NextInspectionDueAtUtc")
            .AddAggregateProperty("DeficiencyCount", aggregate, "DeficiencyCount")
            .AddAggregateProperty("GuardIntegrityScore", aggregate, "GuardIntegrityScore")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_MachineGuardsBarriers_Update]", parameters, cancellationToken: cancellationToken);
    }

    /// <inheritdoc cref="IReadRepo.GetMachineGuardsBarriersItemsAsync"/>
    public async Task<IReadOnlyList<MachineGuardsBarriersItemDto>> GetMachineGuardsBarriersItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetMachineGuardsBarriersItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<MachineGuardsBarriersItemDto>(
            $"[{Schema}].[usp_MachineGuardsBarriers_GetItems]", parameters, cancellationToken);
    }
}
