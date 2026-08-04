// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.SafetyZones
//  Layer:          Infrastructure
//  File:           SafetyZoneDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the SafetyZone aggregate. Implements both the
//                  Domain-layer write repository and the Application-layer read-model
//                  repository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Specifications;
using PlantSafety.Domain.PhysicalAccessControl.SafetyZones.ValueObjects.Ids;
using PlantSafety.Application.PhysicalAccessControl.SafetyZones.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates.SafetyZone;
using IWriteRepo = PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Interfaces.ISafetyZoneRepository;
using IReadRepo = PlantSafety.Application.PhysicalAccessControl.SafetyZones.Repositories.ISafetyZoneReadRepository;

namespace PlantSafety.Infrastructure.PhysicalAccessControl.SafetyZones.Persistence;

/// <summary>Flat Dapper row for [PhysicalAccessControl].[SafetyZones], column-for-column with the SafetyZone aggregate.</summary>
internal sealed class SafetyZoneRow
{
    public Guid SafetyZoneId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? ZoneCode { get; init; }
    public string? ZoneName { get; init; }
    public string? HazardLevel { get; init; }
    public int MaxOccupancy { get; init; }
    public int CurrentOccupancy { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the SafetyZone aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class SafetyZoneDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<SafetyZoneDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "PhysicalAccessControl";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(SafetyZoneId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for SafetyZone {SafetyZoneId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("SafetyZoneId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<SafetyZoneRow>(
            $"[{Schema}].[usp_SafetyZones_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, SafetyZoneRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(SafetyZoneSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for SafetyZone");

        var rows = await queryExecutor.QueryAsync<SafetyZoneRow>(
            $"[{Schema}].[usp_SafetyZones_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, SafetyZoneRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for SafetyZone {SafetyZoneId}", aggregate.SafetyZoneId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("SafetyZoneId", aggregate, "SafetyZoneId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ZoneCode", aggregate, "ZoneCode")
            .AddAggregateProperty("ZoneName", aggregate, "ZoneName")
            .AddAggregateProperty("HazardLevel", aggregate, "HazardLevel")
            .AddAggregateProperty("MaxOccupancy", aggregate, "MaxOccupancy")
            .AddAggregateProperty("CurrentOccupancy", aggregate, "CurrentOccupancy")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_SafetyZones_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for SafetyZone {SafetyZoneId}", aggregate.SafetyZoneId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("SafetyZoneId", aggregate, "SafetyZoneId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ZoneCode", aggregate, "ZoneCode")
            .AddAggregateProperty("ZoneName", aggregate, "ZoneName")
            .AddAggregateProperty("HazardLevel", aggregate, "HazardLevel")
            .AddAggregateProperty("MaxOccupancy", aggregate, "MaxOccupancy")
            .AddAggregateProperty("CurrentOccupancy", aggregate, "CurrentOccupancy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_SafetyZones_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetOccupancyAsync"/>
    public async Task<SafetyZoneOccupancyDto?> GetOccupancyAsync(Guid tenantId, Guid safetyZoneId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetOccupancyAsync (read-model) for SafetyZone {SafetyZoneId}", safetyZoneId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SafetyZoneId", safetyZoneId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<SafetyZoneOccupancyDto>(
            $"[{Schema}].[usp_SafetyZones_GetOccupancyItem]", parameters, cancellationToken);
    }
}
