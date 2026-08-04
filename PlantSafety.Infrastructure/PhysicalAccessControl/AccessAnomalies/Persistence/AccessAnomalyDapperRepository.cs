// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessAnomalies
//  Layer:          Infrastructure
//  File:           AccessAnomalyDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the AccessAnomaly aggregate. Implements both the
//                  Domain-layer write repository and the Application-layer read-model
//                  repository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Specifications;
using PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.ValueObjects.Ids;
using PlantSafety.Application.PhysicalAccessControl.AccessAnomalies.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Aggregates.AccessAnomaly;
using IWriteRepo = PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Interfaces.IAccessAnomalyRepository;
using IReadRepo = PlantSafety.Application.PhysicalAccessControl.AccessAnomalies.Repositories.IAccessAnomalyReadRepository;

namespace PlantSafety.Infrastructure.PhysicalAccessControl.AccessAnomalies.Persistence;

/// <summary>Flat Dapper row for [PhysicalAccessControl].[AccessAnomalies], column-for-column with the AccessAnomaly aggregate.</summary>
internal sealed class AccessAnomalyRow
{
    public Guid AccessAnomalyId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public Guid? AccessEventId { get; init; }
    public Guid AccessPointId { get; init; }
    public Guid? CredentialId { get; init; }
    public Guid? SafetyZoneId { get; init; }
    public string? AnomalyType { get; init; }
    public decimal SeverityScore { get; init; }
    public Guid? AcknowledgedByUserId { get; init; }
    public string? Resolution { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the AccessAnomaly aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class AccessAnomalyDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<AccessAnomalyDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "PhysicalAccessControl";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(AccessAnomalyId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for AccessAnomaly {AccessAnomalyId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AccessAnomalyId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<AccessAnomalyRow>(
            $"[{Schema}].[usp_AccessAnomalies_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, AccessAnomalyRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(AccessAnomalySpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for AccessAnomaly");

        var rows = await queryExecutor.QueryAsync<AccessAnomalyRow>(
            $"[{Schema}].[usp_AccessAnomalies_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, AccessAnomalyRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for AccessAnomaly {AccessAnomalyId}", aggregate.AccessAnomalyId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("AccessAnomalyId", aggregate, "AccessAnomalyId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AccessEventId", aggregate, "AccessEventId")
            .AddAggregateProperty("AccessPointId", aggregate, "AccessPointId")
            .AddAggregateProperty("CredentialId", aggregate, "CredentialId")
            .AddAggregateProperty("SafetyZoneId", aggregate, "SafetyZoneId")
            .AddAggregateProperty("AnomalyType", aggregate, "AnomalyType")
            .AddAggregateProperty("SeverityScore", aggregate, "SeverityScore")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_AccessAnomalies_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for AccessAnomaly {AccessAnomalyId}", aggregate.AccessAnomalyId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("AccessAnomalyId", aggregate, "AccessAnomalyId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AnomalyType", aggregate, "AnomalyType")
            .AddAggregateProperty("SeverityScore", aggregate, "SeverityScore")
            .AddAggregateProperty("AcknowledgedByUserId", aggregate, "AcknowledgedByUserId")
            .AddAggregateProperty("Resolution", aggregate, "Resolution")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_AccessAnomalies_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.ListActiveAsync"/>
    public async Task<IReadOnlyList<AccessAnomalyDto>> ListActiveAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken)
    {
        logger.LogDebug("ListActiveAsync (read-model) for Tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .Build();

        return await queryExecutor.QueryAsync<AccessAnomalyDto>(
            $"[{Schema}].[usp_AccessAnomalies_ListActiveItems]", parameters, cancellationToken);
    }
}
