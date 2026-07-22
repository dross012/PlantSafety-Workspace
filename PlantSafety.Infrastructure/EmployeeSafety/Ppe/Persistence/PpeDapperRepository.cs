// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.Ppe
//  Layer:          Infrastructure
//  File:           PpeDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Ppe aggregate. Implements both the
//                  Domain-layer write repository (IPpeRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetPpeItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  Ppe exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.EmployeeSafety.Ppe.Specifications;
using PlantSafety.Domain.EmployeeSafety.Ppe.ValueObjects.Ids;
using PlantSafety.Application.EmployeeSafety.Ppe.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.EmployeeSafety.Ppe.Aggregates.Ppe;
using IWriteRepo = PlantSafety.Domain.EmployeeSafety.Ppe.Interfaces.IPpeRepository;
using IReadRepo = PlantSafety.Application.EmployeeSafety.Ppe.Repositories.IPpeRepository;

namespace PlantSafety.Infrastructure.EmployeeSafety.Ppe.Persistence;

/// <summary>Flat Dapper row for [EmployeeSafety].[Ppe], column-for-column with the Ppe aggregate.</summary>
internal sealed class PpeRow
{
    public Guid PpeId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? ItemDescription { get; init; }
    public string? Category { get; init; }
    public string? AssignedZone { get; init; }
    public Guid IssuedToUserId { get; init; }
    public DateTime? IssuedAtUtc { get; init; }
    public bool RequiresFitTest { get; init; }
    public DateTime? FitTestedAtUtc { get; init; }
    public int ReplacementCycleDays { get; init; }
    public DateTime? NextReplacementDueAtUtc { get; init; }
    public DateTime? LastInspectedAtUtc { get; init; }
    public decimal? ConditionScore { get; init; }
    public string? ReplacementReason { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the Ppe aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class PpeDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PpeDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "EmployeeSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(PpeId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Ppe {PpeId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PpeId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<PpeRow>(
            $"[{Schema}].[usp_Ppe_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, PpeRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(PpeSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for Ppe");

        var rows = await queryExecutor.QueryAsync<PpeRow>(
            $"[{Schema}].[usp_Ppe_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, PpeRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for Ppe {PpeId}", aggregate.PpeId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("PpeId", aggregate, "PpeId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ItemDescription", aggregate, "ItemDescription")
            .AddAggregateProperty("Category", aggregate, "Category")
            .AddAggregateProperty("AssignedZone", aggregate, "AssignedZone")
            .AddAggregateProperty("IssuedToUserId", aggregate, "IssuedToUserId")
            .AddAggregateProperty("IssuedAtUtc", aggregate, "IssuedAtUtc")
            .AddAggregateProperty("RequiresFitTest", aggregate, "RequiresFitTest")
            .AddAggregateProperty("FitTestedAtUtc", aggregate, "FitTestedAtUtc")
            .AddAggregateProperty("ReplacementCycleDays", aggregate, "ReplacementCycleDays")
            .AddAggregateProperty("NextReplacementDueAtUtc", aggregate, "NextReplacementDueAtUtc")
            .AddAggregateProperty("LastInspectedAtUtc", aggregate, "LastInspectedAtUtc")
            .AddAggregateProperty("ConditionScore", aggregate, "ConditionScore")
            .AddAggregateProperty("ReplacementReason", aggregate, "ReplacementReason")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_Ppe_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for Ppe {PpeId}", aggregate.PpeId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("PpeId", aggregate, "PpeId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ItemDescription", aggregate, "ItemDescription")
            .AddAggregateProperty("Category", aggregate, "Category")
            .AddAggregateProperty("AssignedZone", aggregate, "AssignedZone")
            .AddAggregateProperty("IssuedToUserId", aggregate, "IssuedToUserId")
            .AddAggregateProperty("IssuedAtUtc", aggregate, "IssuedAtUtc")
            .AddAggregateProperty("RequiresFitTest", aggregate, "RequiresFitTest")
            .AddAggregateProperty("FitTestedAtUtc", aggregate, "FitTestedAtUtc")
            .AddAggregateProperty("ReplacementCycleDays", aggregate, "ReplacementCycleDays")
            .AddAggregateProperty("NextReplacementDueAtUtc", aggregate, "NextReplacementDueAtUtc")
            .AddAggregateProperty("LastInspectedAtUtc", aggregate, "LastInspectedAtUtc")
            .AddAggregateProperty("ConditionScore", aggregate, "ConditionScore")
            .AddAggregateProperty("ReplacementReason", aggregate, "ReplacementReason")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_Ppe_Update]", parameters, cancellationToken: cancellationToken);
    }

    /// <inheritdoc cref="IReadRepo.GetPpeItemsAsync"/>
    public async Task<IReadOnlyList<PpeItemDto>> GetPpeItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetPpeItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PpeItemDto>(
            $"[{Schema}].[usp_Ppe_GetItems]", parameters, cancellationToken);
    }
}
