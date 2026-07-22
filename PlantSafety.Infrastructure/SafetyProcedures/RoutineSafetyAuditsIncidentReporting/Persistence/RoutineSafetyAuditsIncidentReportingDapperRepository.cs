// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
//  Layer:          Infrastructure
//  File:           RoutineSafetyAuditsIncidentReportingDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the RoutineSafetyAuditsIncidentReporting aggregate. Implements both the
//                  Domain-layer write repository (IRoutineSafetyAuditsIncidentReportingRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetRoutineSafetyAuditsIncidentReportingItemsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  RoutineSafetyAuditsIncidentReporting exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Specifications;
using PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.ValueObjects.Ids;
using PlantSafety.Application.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Aggregates.RoutineSafetyAuditsIncidentReporting;
using IWriteRepo = PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Interfaces.IRoutineSafetyAuditsIncidentReportingRepository;
using IReadRepo = PlantSafety.Application.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Repositories.IRoutineSafetyAuditsIncidentReportingRepository;

namespace PlantSafety.Infrastructure.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Persistence;

/// <summary>Flat Dapper row for [SafetyProcedures].[RoutineSafetyAuditsIncidentReporting], column-for-column with the RoutineSafetyAuditsIncidentReporting aggregate.</summary>
internal sealed class RoutineSafetyAuditsIncidentReportingRow
{
    public Guid RoutineSafetyAuditsIncidentReportingId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? AuditTitle { get; init; }
    public string? ReportingCategory { get; init; }
    public string? FacilityArea { get; init; }
    public Guid ConductedByUserId { get; init; }
    public DateTime? ConductedAtUtc { get; init; }
    public DateTime? NextAuditDueAtUtc { get; init; }
    public int FindingCount { get; init; }
    public decimal? ComplianceScore { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the RoutineSafetyAuditsIncidentReporting aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class RoutineSafetyAuditsIncidentReportingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<RoutineSafetyAuditsIncidentReportingDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "SafetyProcedures";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(RoutineSafetyAuditsIncidentReportingId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for RoutineSafetyAuditsIncidentReporting {RoutineSafetyAuditsIncidentReportingId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RoutineSafetyAuditsIncidentReportingId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<RoutineSafetyAuditsIncidentReportingRow>(
            $"[{Schema}].[usp_RoutineSafetyAuditsIncidentReporting_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, RoutineSafetyAuditsIncidentReportingRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(RoutineSafetyAuditsIncidentReportingSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for RoutineSafetyAuditsIncidentReporting");

        var rows = await queryExecutor.QueryAsync<RoutineSafetyAuditsIncidentReportingRow>(
            $"[{Schema}].[usp_RoutineSafetyAuditsIncidentReporting_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, RoutineSafetyAuditsIncidentReportingRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for RoutineSafetyAuditsIncidentReporting {RoutineSafetyAuditsIncidentReportingId}", aggregate.RoutineSafetyAuditsIncidentReportingId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("RoutineSafetyAuditsIncidentReportingId", aggregate, "RoutineSafetyAuditsIncidentReportingId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AuditTitle", aggregate, "AuditTitle")
            .AddAggregateProperty("ReportingCategory", aggregate, "ReportingCategory")
            .AddAggregateProperty("FacilityArea", aggregate, "FacilityArea")
            .AddAggregateProperty("ConductedByUserId", aggregate, "ConductedByUserId")
            .AddAggregateProperty("ConductedAtUtc", aggregate, "ConductedAtUtc")
            .AddAggregateProperty("NextAuditDueAtUtc", aggregate, "NextAuditDueAtUtc")
            .AddAggregateProperty("FindingCount", aggregate, "FindingCount")
            .AddAggregateProperty("ComplianceScore", aggregate, "ComplianceScore")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_RoutineSafetyAuditsIncidentReporting_Create]", parameters, cancellationToken: cancellationToken);
    
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
        logger.LogDebug("UpdateAsync for RoutineSafetyAuditsIncidentReporting {RoutineSafetyAuditsIncidentReportingId}", aggregate.RoutineSafetyAuditsIncidentReportingId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("RoutineSafetyAuditsIncidentReportingId", aggregate, "RoutineSafetyAuditsIncidentReportingId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AuditTitle", aggregate, "AuditTitle")
            .AddAggregateProperty("ReportingCategory", aggregate, "ReportingCategory")
            .AddAggregateProperty("FacilityArea", aggregate, "FacilityArea")
            .AddAggregateProperty("ConductedByUserId", aggregate, "ConductedByUserId")
            .AddAggregateProperty("ConductedAtUtc", aggregate, "ConductedAtUtc")
            .AddAggregateProperty("NextAuditDueAtUtc", aggregate, "NextAuditDueAtUtc")
            .AddAggregateProperty("FindingCount", aggregate, "FindingCount")
            .AddAggregateProperty("ComplianceScore", aggregate, "ComplianceScore")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_RoutineSafetyAuditsIncidentReporting_Update]", parameters, cancellationToken: cancellationToken);
    }

    /// <inheritdoc cref="IReadRepo.GetRoutineSafetyAuditsIncidentReportingItemsAsync"/>
    public async Task<IReadOnlyList<RoutineSafetyAuditsIncidentReportingItemDto>> GetRoutineSafetyAuditsIncidentReportingItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRoutineSafetyAuditsIncidentReportingItemsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<RoutineSafetyAuditsIncidentReportingItemDto>(
            $"[{Schema}].[usp_RoutineSafetyAuditsIncidentReporting_GetItems]", parameters, cancellationToken);
    }
}
