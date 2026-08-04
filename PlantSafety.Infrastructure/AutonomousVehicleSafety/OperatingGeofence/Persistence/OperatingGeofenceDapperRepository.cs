// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.OperatingGeofence
//  Layer:          Infrastructure
//  File:           OperatingGeofenceDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the OperatingGeofence aggregate. Implements both the
//                  Domain-layer write repository (IOperatingGeofenceRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetActiveGeofenceViolationsAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Specifications;
using PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.ValueObjects.Ids;
using PlantSafety.Application.AutonomousVehicleSafety.OperatingGeofence.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Aggregates.OperatingGeofence;
using IWriteRepo = PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Interfaces.IOperatingGeofenceRepository;
using IReadRepo = PlantSafety.Application.AutonomousVehicleSafety.OperatingGeofence.Repositories.IOperatingGeofenceRepository;

namespace PlantSafety.Infrastructure.AutonomousVehicleSafety.OperatingGeofence.Persistence;

/// <summary>Flat Dapper row for [AutonomousVehicleSafety].[OperatingGeofence], column-for-column with the OperatingGeofence aggregate.</summary>
internal sealed class OperatingGeofenceRow
{
    public Guid OperatingGeofenceId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? GeofenceName { get; init; }
    public string? ZoneType { get; init; }
    public string? BoundaryDefinition { get; init; }
    public decimal? MaxSpeedKph { get; init; }
    public int ViolationCount { get; init; }
    public DateTime? LastAuditedAtUtc { get; init; }
    public DateTime? NextAuditDueAtUtc { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the OperatingGeofence aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class OperatingGeofenceDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<OperatingGeofenceDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "AutonomousVehicleSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(OperatingGeofenceId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for OperatingGeofence {OperatingGeofenceId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("OperatingGeofenceId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<OperatingGeofenceRow>(
            $"[{Schema}].[usp_OperatingGeofence_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, OperatingGeofenceRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(OperatingGeofenceSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for OperatingGeofence");

        var rows = await queryExecutor.QueryAsync<OperatingGeofenceRow>(
            $"[{Schema}].[usp_OperatingGeofence_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, OperatingGeofenceRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for OperatingGeofence {OperatingGeofenceId}", aggregate.OperatingGeofenceId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("OperatingGeofenceId", aggregate, "OperatingGeofenceId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("GeofenceName", aggregate, "GeofenceName")
            .AddAggregateProperty("ZoneType", aggregate, "ZoneType")
            .AddAggregateProperty("BoundaryDefinition", aggregate, "BoundaryDefinition")
            .AddAggregateProperty("MaxSpeedKph", aggregate, "MaxSpeedKph")
            .AddAggregateProperty("ViolationCount", aggregate, "ViolationCount")
            .AddAggregateProperty("LastAuditedAtUtc", aggregate, "LastAuditedAtUtc")
            .AddAggregateProperty("NextAuditDueAtUtc", aggregate, "NextAuditDueAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_OperatingGeofence_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for OperatingGeofence {OperatingGeofenceId}", aggregate.OperatingGeofenceId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("OperatingGeofenceId", aggregate, "OperatingGeofenceId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("GeofenceName", aggregate, "GeofenceName")
            .AddAggregateProperty("ZoneType", aggregate, "ZoneType")
            .AddAggregateProperty("BoundaryDefinition", aggregate, "BoundaryDefinition")
            .AddAggregateProperty("MaxSpeedKph", aggregate, "MaxSpeedKph")
            .AddAggregateProperty("ViolationCount", aggregate, "ViolationCount")
            .AddAggregateProperty("LastAuditedAtUtc", aggregate, "LastAuditedAtUtc")
            .AddAggregateProperty("NextAuditDueAtUtc", aggregate, "NextAuditDueAtUtc")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_OperatingGeofence_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetActiveGeofenceViolationsAsync"/>
    public async Task<IReadOnlyList<OperatingGeofenceItemDto>> GetActiveGeofenceViolationsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetActiveGeofenceViolationsAsync for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<OperatingGeofenceItemDto>(
            $"[{Schema}].[usp_OperatingGeofence_GetActiveViolations]", parameters, cancellationToken);
    }
}
