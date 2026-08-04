// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessPoints
//  Layer:          Infrastructure
//  File:           AccessPointDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the AccessPoint aggregate. Implements both the
//                  Domain-layer write repository and the Application-layer read-model
//                  repository. All DB access via stored procedures; no dynamic SQL.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Specifications;
using PlantSafety.Domain.PhysicalAccessControl.AccessPoints.ValueObjects.Ids;
using PlantSafety.Application.PhysicalAccessControl.AccessPoints.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates.AccessPoint;
using IWriteRepo = PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Interfaces.IAccessPointRepository;
using IReadRepo = PlantSafety.Application.PhysicalAccessControl.AccessPoints.Repositories.IAccessPointReadRepository;

namespace PlantSafety.Infrastructure.PhysicalAccessControl.AccessPoints.Persistence;

/// <summary>Flat Dapper row for [PhysicalAccessControl].[AccessPoints], column-for-column with the AccessPoint aggregate.</summary>
internal sealed class AccessPointRow
{
    public Guid AccessPointId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? AccessPointCode { get; init; }
    public string? Description { get; init; }
    public string? DeviceType { get; init; }
    public Guid? SafetyZoneId { get; init; }
    public bool IsPerimeter { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the AccessPoint aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class AccessPointDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<AccessPointDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "PhysicalAccessControl";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(AccessPointId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for AccessPoint {AccessPointId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AccessPointId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<AccessPointRow>(
            $"[{Schema}].[usp_AccessPoints_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, AccessPointRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(AccessPointSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for AccessPoint");

        var rows = await queryExecutor.QueryAsync<AccessPointRow>(
            $"[{Schema}].[usp_AccessPoints_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, AccessPointRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for AccessPoint {AccessPointId}", aggregate.AccessPointId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("AccessPointId", aggregate, "AccessPointId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AccessPointCode", aggregate, "AccessPointCode")
            .AddAggregateProperty("Description", aggregate, "Description")
            .AddAggregateProperty("DeviceType", aggregate, "DeviceType")
            .AddAggregateProperty("SafetyZoneId", aggregate, "SafetyZoneId")
            .AddAggregateProperty("IsPerimeter", aggregate, "IsPerimeter")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_AccessPoints_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for AccessPoint {AccessPointId}", aggregate.AccessPointId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("AccessPointId", aggregate, "AccessPointId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AccessPointCode", aggregate, "AccessPointCode")
            .AddAggregateProperty("Description", aggregate, "Description")
            .AddAggregateProperty("DeviceType", aggregate, "DeviceType")
            .AddAggregateProperty("SafetyZoneId", aggregate, "SafetyZoneId")
            .AddAggregateProperty("IsPerimeter", aggregate, "IsPerimeter")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_AccessPoints_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetByIdAsync"/>
    public async Task<AccessPointDto?> GetByIdAsync(Guid tenantId, Guid accessPointId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetByIdAsync (read-model) for AccessPoint {AccessPointId}", accessPointId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("AccessPointId", accessPointId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<AccessPointDto>(
            $"[{Schema}].[usp_AccessPoints_GetByIdItem]", parameters, cancellationToken);
    }
}
