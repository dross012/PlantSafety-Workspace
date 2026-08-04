// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Infrastructure
//  File:           AutonomousVehicleAssetDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the AutonomousVehicleAsset aggregate. Implements both the
//                  Domain-layer write repository (IAutonomousVehicleAssetRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetAutonomousVehicleAssetByIdAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure) because
//                  AutonomousVehicleAsset exposes only a private parameterless constructor and
//                  private-set properties with no public Domain-layer rehydration factory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Specifications;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.ValueObjects.Ids;
using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleAsset.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Aggregates.AutonomousVehicleAsset;
using IWriteRepo = PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Interfaces.IAutonomousVehicleAssetRepository;
using IReadRepo = PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleAsset.Repositories.IAutonomousVehicleAssetRepository;

namespace PlantSafety.Infrastructure.AutonomousVehicleSafety.AutonomousVehicleAsset.Persistence;

/// <summary>Flat Dapper row for [AutonomousVehicleSafety].[AutonomousVehicleAsset], column-for-column with the AutonomousVehicleAsset aggregate.</summary>
internal sealed class AutonomousVehicleAssetRow
{
    public Guid AutonomousVehicleAssetId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? AssetTag { get; init; }
    public string? VehicleType { get; init; }
    public string? AutonomyLevel { get; init; }
    public string? CurrentAutonomyMode { get; init; }
    public Guid? SafetyCaseValidationId { get; init; }
    public int ModeTransitionCount { get; init; }
    public string? SuspensionReason { get; init; }
    public DateTime? SuspendedAtUtc { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the AutonomousVehicleAsset aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class AutonomousVehicleAssetDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<AutonomousVehicleAssetDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "AutonomousVehicleSafety";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(AutonomousVehicleAssetId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for AutonomousVehicleAsset {AutonomousVehicleAssetId}", id);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AutonomousVehicleAssetId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<AutonomousVehicleAssetRow>(
            $"[{Schema}].[usp_AutonomousVehicleAsset_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, AutonomousVehicleAssetRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(AutonomousVehicleAssetSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("ListAsync for AutonomousVehicleAsset");

        var rows = await queryExecutor.QueryAsync<AutonomousVehicleAssetRow>(
            $"[{Schema}].[usp_AutonomousVehicleAsset_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows
            .Select(AggregateHydrator.Hydrate<Agg, AutonomousVehicleAssetRow>)
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for AutonomousVehicleAsset {AutonomousVehicleAssetId}", aggregate.AutonomousVehicleAssetId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("AutonomousVehicleAssetId", aggregate, "AutonomousVehicleAssetId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AssetTag", aggregate, "AssetTag")
            .AddAggregateProperty("VehicleType", aggregate, "VehicleType")
            .AddAggregateProperty("AutonomyLevel", aggregate, "AutonomyLevel")
            .AddAggregateProperty("CurrentAutonomyMode", aggregate, "CurrentAutonomyMode")
            .AddAggregateProperty("SafetyCaseValidationId", aggregate, "SafetyCaseValidationId")
            .AddAggregateProperty("ModeTransitionCount", aggregate, "ModeTransitionCount")
            .AddAggregateProperty("SuspensionReason", aggregate, "SuspensionReason")
            .AddAggregateProperty("SuspendedAtUtc", aggregate, "SuspendedAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_AutonomousVehicleAsset_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for AutonomousVehicleAsset {AutonomousVehicleAssetId}", aggregate.AutonomousVehicleAssetId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("AutonomousVehicleAssetId", aggregate, "AutonomousVehicleAssetId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("AssetTag", aggregate, "AssetTag")
            .AddAggregateProperty("VehicleType", aggregate, "VehicleType")
            .AddAggregateProperty("AutonomyLevel", aggregate, "AutonomyLevel")
            .AddAggregateProperty("CurrentAutonomyMode", aggregate, "CurrentAutonomyMode")
            .AddAggregateProperty("SafetyCaseValidationId", aggregate, "SafetyCaseValidationId")
            .AddAggregateProperty("ModeTransitionCount", aggregate, "ModeTransitionCount")
            .AddAggregateProperty("SuspensionReason", aggregate, "SuspensionReason")
            .AddAggregateProperty("SuspendedAtUtc", aggregate, "SuspendedAtUtc")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_AutonomousVehicleAsset_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetAutonomousVehicleAssetByIdAsync"/>
    public async Task<AutonomousVehicleAssetItemDto?> GetAutonomousVehicleAssetByIdAsync(
        Guid tenantId,
        Guid autonomousVehicleAssetId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetAutonomousVehicleAssetByIdAsync for Asset {AutonomousVehicleAssetId}, tenant {TenantId}", autonomousVehicleAssetId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("AutonomousVehicleAssetId", autonomousVehicleAssetId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<AutonomousVehicleAssetItemDto>(
            $"[{Schema}].[usp_AutonomousVehicleAsset_GetItemById]", parameters, cancellationToken);
    }
}
