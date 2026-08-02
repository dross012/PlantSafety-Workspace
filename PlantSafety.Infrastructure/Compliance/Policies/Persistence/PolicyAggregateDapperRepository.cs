// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Infrastructure
//  File:           PolicyAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IPolicyRepository (Domain repository interface
//                  for the Policy aggregate root). All DB access via stored procedures in the
//                  [Compliance] schema, against the [Compliance].[PolicyAggregate] table (distinct
//                  from the pre-existing [Compliance].[Policy*] category tables). Aggregate
//                  reconstruction uses AggregateReflectionHydrator because Policy exposes only a
//                  private "for rehydration only" constructor and private-setter properties. Named
//                  "PolicyAggregateDapperRepository" (not "PolicyDapperRepository") to avoid
//                  colliding with the pre-existing, unregistered PolicyRepository orphan class in
//                  this same namespace. Both AddAsync AND UpdateAsync enqueue domain events to the
//                  outbox after their write. GetByIdAsync is tenant-scoped and UpdateAsync enforces
//                  optimistic concurrency via @ETag from day one. Mirrors PermitsDapperRepository.cs.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.Compliance.Policy.Enums;
using PlantSafety.Domain.Compliance.Policy.Events;
using PlantSafety.Domain.Compliance.Policy.Interfaces;
using PlantSafety.Domain.Compliance.Policy.Specifications;
using PlantSafety.Domain.Compliance.Policy.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Compliance.Policies.Persistence;

/// <summary>Dapper-backed repository for the Policy aggregate root.</summary>
public sealed class PolicyAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PolicyAggregateDapperRepository> logger)
    : IPolicyRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Compliance.Policy.Aggregates.Policy?> GetByIdAsync(PolicyId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Policy {PolicyId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PolicyId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PolicyRow>(
            "[Compliance].[usp_Policy_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<PlantSafety.Domain.Compliance.Policy.Aggregates.Policy>> ListAsync(PolicySpecification specification, CancellationToken cancellationToken = default)
    {
        // No bulk "get all active" proc exists yet for this aggregate -- ListAsync is not
        // exercised by any Application-layer command/query in this pass (only GetByIdAsync is).
        logger.LogDebug("List Policy via specification {Specification}", specification.GetType().Name);
        return Task.FromResult<IReadOnlyList<PlantSafety.Domain.Compliance.Policy.Aggregates.Policy>>(Array.Empty<PlantSafety.Domain.Compliance.Policy.Aggregates.Policy>());
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Compliance.Policy.Aggregates.Policy aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add Policy {PolicyId}", aggregate.PolicyId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PolicyId", aggregate.PolicyId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("PolicyTitle", aggregate.PolicyTitle, 400)
            .AddInt("PolicyVersion", aggregate.PolicyVersion)
            .AddString("Category", aggregate.Category, 100)
            .AddGuid("OwnerId", aggregate.OwnerId.Value)
            .AddDateTime("EffectiveAtUtc", aggregate.EffectiveAtUtc?.Value)
            .AddBool("AcknowledgmentRequired", aggregate.AcknowledgmentRequired)
            .AddInt("AcknowledgmentCount", aggregate.AcknowledgmentCount)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Compliance].[usp_Policy_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.Compliance.Policy.Aggregates.Policy aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update Policy {PolicyId}", aggregate.PolicyId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PolicyId", aggregate.PolicyId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("PolicyTitle", aggregate.PolicyTitle, 400)
            .AddInt("PolicyVersion", aggregate.PolicyVersion)
            .AddString("Category", aggregate.Category, 100)
            .AddGuid("OwnerId", aggregate.OwnerId.Value)
            .AddDateTime("EffectiveAtUtc", aggregate.EffectiveAtUtc?.Value)
            .AddBool("AcknowledgmentRequired", aggregate.AcknowledgmentRequired)
            .AddInt("AcknowledgmentCount", aggregate.AcknowledgmentCount)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_Policy_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[Compliance].[usp_Policy_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"Policy {aggregate.PolicyId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a Policy aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Compliance.Policy.Aggregates.Policy Hydrate(PolicyRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Compliance.Policy.Aggregates.Policy>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Policy.Aggregates.Policy.PolicyId), PolicyId.FromGuid(row.PolicyId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Policy.Aggregates.Policy.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Policy.Aggregates.Policy.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Policy.Aggregates.Policy.Status), Enum.Parse<PolicyStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Policy.Aggregates.Policy.PolicyTitle), row.PolicyTitle);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Policy.Aggregates.Policy.PolicyVersion), row.PolicyVersion);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Policy.Aggregates.Policy.Category), row.Category);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Policy.Aggregates.Policy.OwnerId), new UserId(row.OwnerId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Policy.Aggregates.Policy.EffectiveAtUtc), row.EffectiveAtUtc.HasValue ? UtcTimestamp.From(row.EffectiveAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Policy.Aggregates.Policy.AcknowledgmentRequired), row.AcknowledgmentRequired);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Policy.Aggregates.Policy.AcknowledgmentCount), row.AcknowledgmentCount);
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Policy.Aggregates.Policy.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(PlantSafety.Domain.Compliance.Policy.Aggregates.Policy.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPolicyDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_Policy_GetById</c>.</summary>
    private sealed record PolicyRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated by
        /// <c>usp_Policy_GetById</c>. Not consumed by aggregate hydration -- callers obtain the
        /// ETag for a subsequent write from a prior single-record read (see the Application-layer
        /// PolicyItemDto.ETag).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid PolicyId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string PolicyTitle { get; init; }
        public required int PolicyVersion { get; init; }
        public required string Category { get; init; }
        public required Guid OwnerId { get; init; }
        public DateTime? EffectiveAtUtc { get; init; }
        public required bool AcknowledgmentRequired { get; init; }
        public required int AcknowledgmentCount { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
