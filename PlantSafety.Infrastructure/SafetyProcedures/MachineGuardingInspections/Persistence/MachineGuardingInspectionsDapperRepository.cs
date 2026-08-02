// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MachineGuardingInspections
//  Layer:          Infrastructure
//  File:           MachineGuardingInspectionsDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the MachineGuardingInspections aggregate. Implements both the
//                  Domain-layer write repository (IMachineGuardingInspectionsRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetMachineGuardingInspectionsItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  MachineGuardingInspections exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Specifications;
using PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.ValueObjects.Ids;
using PlantSafety.Application.SafetyProcedures.MachineGuardingInspections.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Aggregates.MachineGuardingInspections;
using IWriteRepo = PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Interfaces.IMachineGuardingInspectionsRepository;
using IReadRepo = PlantSafety.Application.SafetyProcedures.MachineGuardingInspections.Repositories.IMachineGuardingInspectionsRepository;

namespace PlantSafety.Infrastructure.SafetyProcedures.MachineGuardingInspections.Persistence;

/// <summary>Flat Dapper row for [SafetyProcedures].[MachineGuardingInspections], column-for-column with the MachineGuardingInspections aggregate.</summary>
internal sealed class MachineGuardingInspectionsRow
{
    public Guid MachineGuardingInspectionsId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? InspectionTitle { get; init; }
    public string? InspectionCategory { get; init; }
    public string? EquipmentLocation { get; init; }
    public Guid InspectedByUserId { get; init; }
    public DateTime? InspectedAtUtc { get; init; }
    public DateTime? NextInspectionDueAtUtc { get; init; }
    public int DeficiencyCount { get; init; }
    public decimal? ComplianceScore { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the MachineGuardingInspections aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class MachineGuardingInspectionsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<MachineGuardingInspectionsDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "SafetyProcedures";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(MachineGuardingInspectionsId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for MachineGuardingInspections {MachineGuardingInspectionsId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("MachineGuardingInspectionsId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<MachineGuardingInspectionsRow>(
            $"[{Schema}].[usp_MachineGuardingInspections_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, MachineGuardingInspectionsRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(MachineGuardingInspectionsSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for MachineGuardingInspections");

        var rows = await queryExecutor.QueryAsync<MachineGuardingInspectionsRow>(
            $"[{Schema}].[usp_MachineGuardingInspections_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, MachineGuardingInspectionsRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for MachineGuardingInspections {MachineGuardingInspectionsId}", aggregate.MachineGuardingInspectionsId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("MachineGuardingInspectionsId", aggregate, "MachineGuardingInspectionsId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("InspectionTitle", aggregate, "InspectionTitle")
            .AddAggregateProperty("InspectionCategory", aggregate, "InspectionCategory")
            .AddAggregateProperty("EquipmentLocation", aggregate, "EquipmentLocation")
            .AddAggregateProperty("InspectedByUserId", aggregate, "InspectedByUserId")
            .AddAggregateProperty("InspectedAtUtc", aggregate, "InspectedAtUtc")
            .AddAggregateProperty("NextInspectionDueAtUtc", aggregate, "NextInspectionDueAtUtc")
            .AddAggregateProperty("DeficiencyCount", aggregate, "DeficiencyCount")
            .AddAggregateProperty("ComplianceScore", aggregate, "ComplianceScore")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_MachineGuardingInspections_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for MachineGuardingInspections {MachineGuardingInspectionsId}", aggregate.MachineGuardingInspectionsId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("MachineGuardingInspectionsId", aggregate, "MachineGuardingInspectionsId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("InspectionTitle", aggregate, "InspectionTitle")
            .AddAggregateProperty("InspectionCategory", aggregate, "InspectionCategory")
            .AddAggregateProperty("EquipmentLocation", aggregate, "EquipmentLocation")
            .AddAggregateProperty("InspectedByUserId", aggregate, "InspectedByUserId")
            .AddAggregateProperty("InspectedAtUtc", aggregate, "InspectedAtUtc")
            .AddAggregateProperty("NextInspectionDueAtUtc", aggregate, "NextInspectionDueAtUtc")
            .AddAggregateProperty("DeficiencyCount", aggregate, "DeficiencyCount")
            .AddAggregateProperty("ComplianceScore", aggregate, "ComplianceScore")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_MachineGuardingInspections_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetMachineGuardingInspectionsItemsAsync"/>
    public async Task<IReadOnlyList<MachineGuardingInspectionsItemDto>> GetMachineGuardingInspectionsItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetMachineGuardingInspectionsItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<MachineGuardingInspectionsItemDto>(
            $"[{Schema}].[usp_MachineGuardingInspections_GetItems]", parameters, cancellationToken);
    }
}
