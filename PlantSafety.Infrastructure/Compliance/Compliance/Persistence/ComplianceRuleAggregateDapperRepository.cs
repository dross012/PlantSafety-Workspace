// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Infrastructure
//  File:           ComplianceRuleAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IComplianceRepository (Domain repository
//                  interface for the self-named Compliance aggregate root). Named
//                  "ComplianceRuleAggregateDapperRepository" -- deliberately NOT
//                  "ComplianceDapperRepository" or "ComplianceRepository" -- per this module's
//                  build-out naming-collision warning: the word "Compliance" is used pervasively
//                  throughout this codebase (top-level module folder, schema, and multiple
//                  unrelated read-only category slices -- Obligations/RegulatoryRequirements/
//                  Tracking -- under this same module folder, plus a pre-existing dead
//                  ComplianceRepository/ComplianceReadModel scaffold in this same namespace). All
//                  DB access via stored procedures in the [Compliance] schema, against
//                  [Compliance].[ComplianceRuleAggregate]. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because the aggregate exposes only a private "for
//                  rehydration only" constructor and private-setter properties. Both AddAsync AND
//                  UpdateAsync enqueue domain events to the outbox after their write.
//                  GetByIdAsync is tenant-scoped and UpdateAsync enforces optimistic concurrency
//                  via @ETag. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Permits.Persistence.PermitsDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Compliance.Compliance.Enums;
using PlantSafety.Domain.Compliance.Compliance.Events;
using PlantSafety.Domain.Compliance.Compliance.Interfaces;
using PlantSafety.Domain.Compliance.Compliance.Specifications;
using PlantSafety.Domain.Compliance.Compliance.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
using ComplianceAggregate = PlantSafety.Domain.Compliance.Compliance.Aggregates.Compliance;

namespace PlantSafety.Infrastructure.Compliance.Compliance.Persistence;

/// <summary>Dapper-backed repository for the (self-named) <see cref="ComplianceAggregate"/> aggregate root.</summary>
public sealed class ComplianceRuleAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<ComplianceRuleAggregateDapperRepository> logger)
    : IComplianceRepository
{
    /// <inheritdoc/>
    public async Task<ComplianceAggregate?> GetByIdAsync(ComplianceId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Compliance {ComplianceId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ComplianceId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<ComplianceRow>(
            "[Compliance].[usp_ComplianceRuleAggregate_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ComplianceAggregate>> ListAsync(ComplianceSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List Compliance via specification {Specification}", specification.GetType().Name);

        // No bulk read stored procedure exists for this aggregate yet (only single-record
        // GetById per this build-out's reduced scope); ListAsync returns an empty result until a
        // bulk-read proc is introduced.
        await Task.CompletedTask;
        return Array.Empty<ComplianceAggregate>();
    }

    /// <inheritdoc/>
    public async Task AddAsync(ComplianceAggregate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add Compliance {ComplianceId}", aggregate.ComplianceId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ComplianceId", aggregate.ComplianceId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("RegulationReference", aggregate.RegulationReference, 400)
            .AddString("RuleDescription", aggregate.RuleDescription, 2000)
            .AddDecimal("ComplianceScore", aggregate.ComplianceScore)
            .AddInt("NonConformanceCount", aggregate.NonConformanceCount)
            .AddGuid("ResponsiblePartyId", aggregate.ResponsiblePartyId.Value)
            .AddDateTime("LastEvaluatedAtUtc", aggregate.LastEvaluatedAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Compliance].[usp_ComplianceRuleAggregate_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(ComplianceAggregate aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update Compliance {ComplianceId}", aggregate.ComplianceId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ComplianceId", aggregate.ComplianceId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("RegulationReference", aggregate.RegulationReference, 400)
            .AddString("RuleDescription", aggregate.RuleDescription, 2000)
            .AddDecimal("ComplianceScore", aggregate.ComplianceScore)
            .AddInt("NonConformanceCount", aggregate.NonConformanceCount)
            .AddGuid("ResponsiblePartyId", aggregate.ResponsiblePartyId.Value)
            .AddDateTime("LastEvaluatedAtUtc", aggregate.LastEvaluatedAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_ComplianceRuleAggregate_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Compliance].[usp_ComplianceRuleAggregate_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"Compliance {aggregate.ComplianceId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="ComplianceAggregate"/> from its flat read-model row.</summary>
    private static ComplianceAggregate Hydrate(ComplianceRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<ComplianceAggregate>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(ComplianceAggregate.ComplianceId), ComplianceId.FromGuid(row.ComplianceId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ComplianceAggregate.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ComplianceAggregate.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ComplianceAggregate.Status), Enum.Parse<ComplianceStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ComplianceAggregate.RegulationReference), row.RegulationReference);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ComplianceAggregate.RuleDescription), row.RuleDescription);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ComplianceAggregate.ComplianceScore), row.ComplianceScore);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ComplianceAggregate.NonConformanceCount), row.NonConformanceCount);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ComplianceAggregate.ResponsiblePartyId), new UserId(row.ResponsiblePartyId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ComplianceAggregate.LastEvaluatedAtUtc), row.LastEvaluatedAtUtc.HasValue ? UtcTimestamp.From(row.LastEvaluatedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ComplianceAggregate.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ComplianceAggregate.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IComplianceDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_ComplianceRuleAggregate_GetById</c>.</summary>
    private sealed record ComplianceRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_ComplianceRuleAggregate_GetById</c>. Not consumed by aggregate hydration.
        /// </summary>
        public string? ETag { get; init; }

        public required Guid ComplianceId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string RegulationReference { get; init; }
        public required string RuleDescription { get; init; }
        public decimal ComplianceScore { get; init; }
        public int NonConformanceCount { get; init; }
        public required Guid ResponsiblePartyId { get; init; }
        public DateTime? LastEvaluatedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
