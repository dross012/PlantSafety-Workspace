// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessEvents
//  Layer:          Infrastructure
//  File:           AccessEventDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the AccessEvent aggregate. Implements both the
//                  Domain-layer write repository and the Application-layer read-model
//                  repository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Specifications;
using PlantSafety.Domain.PhysicalAccessControl.AccessEvents.ValueObjects.Ids;
using PlantSafety.Application.PhysicalAccessControl.AccessEvents.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates.AccessEvent;
using IWriteRepo = PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Interfaces.IAccessEventRepository;
using IReadRepo = PlantSafety.Application.PhysicalAccessControl.AccessEvents.Repositories.IAccessEventReadRepository;

namespace PlantSafety.Infrastructure.PhysicalAccessControl.AccessEvents.Persistence;

/// <summary>Flat Dapper row for [PhysicalAccessControl].[AccessEvents], column-for-column with the AccessEvent aggregate.</summary>
internal sealed class AccessEventRow
{
    public Guid AccessEventId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public Guid AccessPointId { get; init; }
    public Guid? CredentialId { get; init; }
    public Guid? SafetyZoneId { get; init; }
    public string? EventType { get; init; }
    public string? Direction { get; init; }
    public DateTime? OccurredAtUtc { get; init; }
    public string? FlaggedAnomalyType { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the AccessEvent aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class AccessEventDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<AccessEventDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "PhysicalAccessControl";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(AccessEventId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for AccessEvent {AccessEventId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AccessEventId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<AccessEventRow>(
            $"[{Schema}].[usp_AccessEvents_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, AccessEventRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(AccessEventSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for AccessEvent");

        var rows = await queryExecutor.QueryAsync<AccessEventRow>(
            $"[{Schema}].[usp_AccessEvents_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, AccessEventRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for AccessEvent {AccessEventId}", aggregate.AccessEventId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("AccessEventId", aggregate, "AccessEventId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AccessPointId", aggregate, "AccessPointId")
            .AddAggregateProperty("CredentialId", aggregate, "CredentialId")
            .AddAggregateProperty("SafetyZoneId", aggregate, "SafetyZoneId")
            .AddAggregateProperty("EventType", aggregate, "EventType")
            .AddAggregateProperty("Direction", aggregate, "Direction")
            .AddAggregateProperty("OccurredAtUtc", aggregate, "OccurredAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_AccessEvents_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for AccessEvent {AccessEventId}", aggregate.AccessEventId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("AccessEventId", aggregate, "AccessEventId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AccessPointId", aggregate, "AccessPointId")
            .AddAggregateProperty("CredentialId", aggregate, "CredentialId")
            .AddAggregateProperty("SafetyZoneId", aggregate, "SafetyZoneId")
            .AddAggregateProperty("EventType", aggregate, "EventType")
            .AddAggregateProperty("Direction", aggregate, "Direction")
            .AddAggregateProperty("FlaggedAnomalyType", aggregate, "FlaggedAnomalyType")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_AccessEvents_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.ListByZoneAsync"/>
    public async Task<IReadOnlyList<AccessEventDto>> ListByZoneAsync(
        Guid tenantId, Guid safetyZoneId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        logger.LogDebug("ListByZoneAsync (read-model) for SafetyZone {SafetyZoneId}", safetyZoneId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SafetyZoneId", safetyZoneId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<AccessEventDto>(
            $"[{Schema}].[usp_AccessEvents_ListByZoneItems]", parameters, cancellationToken);
    }
}
