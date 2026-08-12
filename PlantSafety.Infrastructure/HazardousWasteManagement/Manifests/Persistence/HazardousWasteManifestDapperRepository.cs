// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Manifests
//  Layer:          Infrastructure
//  File:           HazardousWasteManifestDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Dapper repository for the HazardousWasteManifest aggregate. Implements both
//                  the Domain-layer write repository and the Application-layer read-model
//                  repository against the same table. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.Manifests.Specifications;
using PlantSafety.Domain.HazardousWasteManagement.Manifests.ValueObjects.Ids;
using PlantSafety.Application.HazardousWasteManagement.Manifests.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.HazardousWasteManagement.Manifests.Aggregates.HazardousWasteManifest;
using IWriteRepo = PlantSafety.Domain.HazardousWasteManagement.Manifests.Interfaces.IHazardousWasteManifestRepository;
using IReadRepo = PlantSafety.Application.HazardousWasteManagement.Manifests.Repositories.IHazardousWasteManifestRepository;

namespace PlantSafety.Infrastructure.HazardousWasteManagement.Manifests.Persistence;

/// <summary>Flat Dapper row for [HazardousWasteManagement].[Manifests], column-for-column with the HazardousWasteManifest aggregate.</summary>
internal sealed class HazardousWasteManifestRow
{
    public Guid HazardousWasteManifestId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? ManifestNumber { get; init; }
    public string? GeneratorEpaId { get; init; }
    public Guid WasteStreamId { get; init; }
    public Guid? TransporterId { get; init; }
    public Guid? TsdfFacilityId { get; init; }
    public DateTime? GeneratedAtUtc { get; init; }
    public DateTime? TransporterSignedAtUtc { get; init; }
    public DateTime? TsdfReceivedAtUtc { get; init; }
    public DateTime? ClosedAtUtc { get; init; }
    public bool IsImmutable { get; init; }
    public string? DiscrepancyReason { get; init; }
    public DateTime? RetentionExpiresAtUtc { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>Dapper-backed repository for the HazardousWasteManifest aggregate.</summary>
public sealed class HazardousWasteManifestDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "HazardousWasteManagement";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(HazardousWasteManifestId id, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("HazardousWasteManifestId", id.Value).Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<HazardousWasteManifestRow>(
            $"[{Schema}].[usp_Manifests_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, HazardousWasteManifestRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(HazardousWasteManifestSpecification specification, CancellationToken cancellationToken = default)
    {
        var rows = await queryExecutor.QueryAsync<HazardousWasteManifestRow>(
            $"[{Schema}].[usp_Manifests_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows.Select(AggregateHydrator.Hydrate<Agg, HazardousWasteManifestRow>).Where(predicate).ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("HazardousWasteManifestId", aggregate, "HazardousWasteManifestId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ManifestNumber", aggregate, "ManifestNumber")
            .AddAggregateProperty("GeneratorEpaId", aggregate, "GeneratorEpaId")
            .AddAggregateProperty("WasteStreamId", aggregate, "WasteStreamId")
            .AddAggregateProperty("GeneratedAtUtc", aggregate, "GeneratedAtUtc")
            .AddAggregateProperty("IsImmutable", aggregate, "IsImmutable")
            .AddAggregateProperty("RetentionExpiresAtUtc", aggregate, "RetentionExpiresAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_Manifests_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("HazardousWasteManifestId", aggregate, "HazardousWasteManifestId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("TransporterId", aggregate, "TransporterId")
            .AddAggregateProperty("TsdfFacilityId", aggregate, "TsdfFacilityId")
            .AddAggregateProperty("TransporterSignedAtUtc", aggregate, "TransporterSignedAtUtc")
            .AddAggregateProperty("TsdfReceivedAtUtc", aggregate, "TsdfReceivedAtUtc")
            .AddAggregateProperty("ClosedAtUtc", aggregate, "ClosedAtUtc")
            .AddAggregateProperty("IsImmutable", aggregate, "IsImmutable")
            .AddAggregateProperty("DiscrepancyReason", aggregate, "DiscrepancyReason")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_Manifests_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetManifestByIdAsync"/>
    public async Task<HazardousWasteManifestItemDto?> GetManifestByIdAsync(Guid tenantId, Guid manifestId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("HazardousWasteManifestId", manifestId).Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<HazardousWasteManifestRow>(
            $"[{Schema}].[usp_Manifests_GetById]", parameters, cancellationToken);

        if (row is null || row.TenantId != tenantId) return null;

        return MapToDto(row);
    }

    /// <inheritdoc cref="IReadRepo.ListOpenManifestsAsync"/>
    public async Task<IReadOnlyList<HazardousWasteManifestItemDto>> ListOpenManifestsAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).AddGuid("SiteId", siteId).Build();

        var rows = await queryExecutor.QueryAsync<HazardousWasteManifestRow>(
            $"[{Schema}].[usp_Manifests_ListOpen]", parameters, cancellationToken);

        return rows.Select(MapToDto).ToList();
    }

    private static HazardousWasteManifestItemDto MapToDto(HazardousWasteManifestRow row) => new()
    {
        HazardousWasteManifestId = row.HazardousWasteManifestId,
        TenantId = row.TenantId,
        SiteId = row.SiteId,
        ManifestNumber = row.ManifestNumber ?? string.Empty,
        GeneratorEpaId = row.GeneratorEpaId ?? string.Empty,
        WasteStreamId = row.WasteStreamId,
        TransporterId = row.TransporterId,
        TsdfFacilityId = row.TsdfFacilityId,
        Status = row.Status ?? string.Empty,
        GeneratedAtUtc = row.GeneratedAtUtc ?? default,
        TransporterSignedAtUtc = row.TransporterSignedAtUtc,
        TsdfReceivedAtUtc = row.TsdfReceivedAtUtc,
        ClosedAtUtc = row.ClosedAtUtc,
        IsImmutable = row.IsImmutable,
        DiscrepancyReason = row.DiscrepancyReason,
        RetentionExpiresAtUtc = row.RetentionExpiresAtUtc ?? default,
    };
}
