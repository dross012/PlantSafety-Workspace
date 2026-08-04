// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity
//  Layer:          Infrastructure
//  File:           OtAssetDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed repository for the OtAsset aggregate root. Implements both the
//                  Domain write-side repository (IOtAssetRepository) and the Application read-side
//                  repository (IOtAssetReadRepository), mirroring the
//                  RiskManagementCategoryDapperRepository dual-interface precedent. Access to raw
//                  OT asset data is restricted to authorized security roles at the API layer
//                  (Authorize policies) -- this repository performs no additional row-level
//                  filtering beyond standard tenant scoping.
// ============================================================================
using PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Models;
using PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Repositories;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Aggregates;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Events;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Interfaces;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Specifications;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
using PlantSafety.Infrastructure.Persistence.UnitOfWork;

namespace PlantSafety.Infrastructure.CyberPhysicalSecurity.Persistence;

/// <summary>Dapper-backed repository for the <see cref="OtAsset"/> aggregate root.</summary>
public sealed class OtAssetDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<OtAssetDapperRepository> logger)
    : IOtAssetRepository, IOtAssetReadRepository
{
    /// <inheritdoc cref="IOtAssetRepository.GetByIdAsync"/>
    public async Task<OtAsset?> GetByIdAsync(OtAssetId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("OtAssetId", id.Value).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<OtAssetRow>(
            "[CyberPhysicalSecurity].[usp_OtAsset_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    async Task<OtAssetDto?> IOtAssetReadRepository.GetByIdAsync(Guid otAssetId, Guid tenantId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("OtAssetId", otAssetId).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<OtAssetRow>(
            "[CyberPhysicalSecurity].[usp_OtAsset_GetById]", parameters, cancellationToken);

        return row is null ? null : ToDto(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<OtAsset>> ListAsync(Guid tenantId, OtAssetSpecification specification, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).Build();
        var rows = await queryExecutor.QueryAsync<OtAssetRow>("[CyberPhysicalSecurity].[usp_OtAsset_GetAllActive]", parameters, cancellationToken);

        var predicate = specification.ToPredicate();
        return rows.Select(Hydrate).Where(predicate).ToList();
    }

    /// <inheritdoc/>
    public async Task AddAsync(OtAsset aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add OtAsset {OtAssetId}", aggregate.OtAssetId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("OtAssetId", aggregate.OtAssetId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("AssetTag", aggregate.AssetTag, 128)
            .AddString("AssetName", aggregate.AssetName, 256)
            .AddString("AssetType", aggregate.AssetType.ToString(), 30)
            .AddString("Vendor", aggregate.Vendor, 128)
            .AddString("Model", aggregate.Model, 128)
            .AddString("FirmwareVersion", aggregate.FirmwareVersion, 64)
            .AddString("NetworkZone", aggregate.NetworkZone, 64)
            .AddString("Criticality", aggregate.Criticality.ToString(), 20)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddDateTime("LastReconciledAtUtc", aggregate.LastReconciledAtUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[CyberPhysicalSecurity].[usp_OtAsset_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        FlushDomainEvents(aggregate);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(OtAsset aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update OtAsset {OtAssetId}", aggregate.OtAssetId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("OtAssetId", aggregate.OtAssetId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddDateTime("LastReconciledAtUtc", aggregate.LastReconciledAtUtc?.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[CyberPhysicalSecurity].[usp_OtAsset_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        FlushDomainEvents(aggregate);
    }

    private void FlushDomainEvents(OtAsset aggregate)
    {
        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());

        aggregate.ClearDomainEvents();
    }

    private static OtAsset Hydrate(OtAssetRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<OtAsset>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(OtAsset.OtAssetId), OtAssetId.FromGuid(row.OtAssetId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(OtAsset.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(OtAsset.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(OtAsset.AssetTag), row.AssetTag);
        AggregateReflectionHydrator.SetProperty(agg, nameof(OtAsset.AssetName), row.AssetName);
        AggregateReflectionHydrator.SetProperty(agg, nameof(OtAsset.AssetType), Enum.Parse<OtAssetType>(row.AssetType));
        AggregateReflectionHydrator.SetProperty(agg, nameof(OtAsset.Vendor), row.Vendor);
        AggregateReflectionHydrator.SetProperty(agg, nameof(OtAsset.Model), row.Model);
        AggregateReflectionHydrator.SetProperty(agg, nameof(OtAsset.FirmwareVersion), row.FirmwareVersion);
        AggregateReflectionHydrator.SetProperty(agg, nameof(OtAsset.NetworkZone), row.NetworkZone);
        AggregateReflectionHydrator.SetProperty(agg, nameof(OtAsset.Criticality), Enum.Parse<OtAssetCriticality>(row.Criticality));
        AggregateReflectionHydrator.SetProperty(agg, nameof(OtAsset.Status), Enum.Parse<OtAssetStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(OtAsset.LastReconciledAtUtc), row.LastReconciledAtUtc.HasValue ? UtcTimestamp.From(row.LastReconciledAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(OtAsset.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(OtAsset.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IOtAssetDomainEvent>());

        return agg;
    }

    private static OtAssetDto ToDto(OtAssetRow row) => new(
        row.OtAssetId, row.TenantId, row.SiteId, row.AssetTag, row.AssetName, row.AssetType,
        row.Vendor, row.Model, row.FirmwareVersion, row.NetworkZone, row.Criticality, row.Status,
        row.LastReconciledAtUtc, row.CreatedAtUtc);

    /// <summary>Flat read-model row shape returned by <c>usp_OtAsset_GetById</c>/<c>usp_OtAsset_GetAllActive</c>.</summary>
    private sealed record OtAssetRow
    {
        public required Guid OtAssetId { get; init; }
        public required Guid TenantId { get; init; }
        public required Guid SiteId { get; init; }
        public required string AssetTag { get; init; }
        public required string AssetName { get; init; }
        public required string AssetType { get; init; }
        public required string Vendor { get; init; }
        public required string Model { get; init; }
        public required string FirmwareVersion { get; init; }
        public required string NetworkZone { get; init; }
        public required string Criticality { get; init; }
        public required string Status { get; init; }
        public DateTime? LastReconciledAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
    }
}
