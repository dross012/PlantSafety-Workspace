// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.RestrictedZonePolicies
//  Layer:          Infrastructure
//  File:           RestrictedZonePolicyDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the RestrictedZonePolicy aggregate. Implements the
//                  Domain-layer write repository. No dedicated Application read-model query is
//                  defined for this entity yet; RestrictedZonePolicy is currently consumed
//                  write-side only (enforcement reads happen through AccessAnomaly detection).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Specifications;
using PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.ValueObjects.Ids;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates.RestrictedZonePolicy;
using IWriteRepo = PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Interfaces.IRestrictedZonePolicyRepository;

namespace PlantSafety.Infrastructure.PhysicalAccessControl.RestrictedZonePolicies.Persistence;

/// <summary>Flat Dapper row for [PhysicalAccessControl].[RestrictedZonePolicies], column-for-column with the RestrictedZonePolicy aggregate.</summary>
internal sealed class RestrictedZonePolicyRow
{
    public Guid RestrictedZonePolicyId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public Guid SafetyZoneId { get; init; }
    public string? PolicyName { get; init; }
    public string? AllowedAccessLevel { get; init; }
    public bool EnforceAfterHoursLockout { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>Dapper-backed repository for the RestrictedZonePolicy aggregate. Implements the Domain write repository.</summary>
public sealed class RestrictedZonePolicyDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<RestrictedZonePolicyDapperRepository> logger)
    : IWriteRepo
{
    private const string Schema = "PhysicalAccessControl";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(RestrictedZonePolicyId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for RestrictedZonePolicy {RestrictedZonePolicyId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RestrictedZonePolicyId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<RestrictedZonePolicyRow>(
            $"[{Schema}].[usp_RestrictedZonePolicies_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, RestrictedZonePolicyRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(RestrictedZonePolicySpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for RestrictedZonePolicy");

        var rows = await queryExecutor.QueryAsync<RestrictedZonePolicyRow>(
            $"[{Schema}].[usp_RestrictedZonePolicies_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, RestrictedZonePolicyRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for RestrictedZonePolicy {RestrictedZonePolicyId}", aggregate.RestrictedZonePolicyId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("RestrictedZonePolicyId", aggregate, "RestrictedZonePolicyId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("SafetyZoneId", aggregate, "SafetyZoneId")
            .AddAggregateProperty("PolicyName", aggregate, "PolicyName")
            .AddAggregateProperty("AllowedAccessLevel", aggregate, "AllowedAccessLevel")
            .AddAggregateProperty("EnforceAfterHoursLockout", aggregate, "EnforceAfterHoursLockout")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_RestrictedZonePolicies_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for RestrictedZonePolicy {RestrictedZonePolicyId}", aggregate.RestrictedZonePolicyId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("RestrictedZonePolicyId", aggregate, "RestrictedZonePolicyId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("SafetyZoneId", aggregate, "SafetyZoneId")
            .AddAggregateProperty("PolicyName", aggregate, "PolicyName")
            .AddAggregateProperty("AllowedAccessLevel", aggregate, "AllowedAccessLevel")
            .AddAggregateProperty("EnforceAfterHoursLockout", aggregate, "EnforceAfterHoursLockout")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_RestrictedZonePolicies_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }
}
