// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
//  Layer:          Infrastructure
//  File:           EvacuationZoneDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the EvacuationZone aggregate. Implements both the
//                  Domain-layer write repository and the Application-layer read-model
//                  repository of the same simple name. Occupancy-reading history is persisted to
//                  the child OccupancyReadings table via an idempotent MERGE upsert per entry.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Specifications;
using PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.ValueObjects.Ids;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationZones.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Aggregates.EvacuationZone;
using IWriteRepo = PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Interfaces.IEvacuationZoneRepository;
using IReadRepo = PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationZones.Repositories.IEvacuationZoneRepository;

namespace PlantSafety.Infrastructure.DigitalTwin.EvacuationModeling.EvacuationZones.Persistence;

/// <summary>Flat Dapper row for [DigitalTwin].[EvacuationZones], column-for-column with the EvacuationZone aggregate.</summary>
internal sealed class EvacuationZoneRow
{
    public Guid EvacuationZoneId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? ZoneName { get; init; }
    public string? TwinModelVersion { get; init; }
    public int ZoneCapacity { get; init; }
    public int OccupancyThreshold { get; init; }
    public int CurrentOccupancyCount { get; init; }
    public DateTime? LastOccupancyReadingAtUtc { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>Flat Dapper row for [DigitalTwin].[OccupancyReadings].</summary>
internal sealed class OccupancyReadingRow
{
    public Guid OccupancyReadingId { get; init; }
    public Guid EvacuationZoneId { get; init; }
    public int HeadcountEstimate { get; init; }
    public string Source { get; init; } = string.Empty;
    public DateTime RecordedAtUtc { get; init; }
    public bool ExceededThreshold { get; init; }
}

/// <summary>
/// Dapper-backed repository for the EvacuationZone aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same tables.
/// </summary>
public sealed class EvacuationZoneDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<EvacuationZoneDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "DigitalTwin";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(EvacuationZoneId id, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("EvacuationZoneId", id.Value).Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<EvacuationZoneRow>(
            $"[{Schema}].[usp_EvacuationZones_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, EvacuationZoneRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(EvacuationZoneSpecification specification, CancellationToken cancellationToken = default)
    {
        var rows = await queryExecutor.QueryAsync<EvacuationZoneRow>(
            $"[{Schema}].[usp_EvacuationZones_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows.Select(AggregateHydrator.Hydrate<Agg, EvacuationZoneRow>).Where(predicate).ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("EvacuationZoneId", aggregate, "EvacuationZoneId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ZoneName", aggregate, "ZoneName")
            .AddAggregateProperty("TwinModelVersion", aggregate, "TwinModelVersion")
            .AddAggregateProperty("ZoneCapacity", aggregate, "ZoneCapacity")
            .AddAggregateProperty("OccupancyThreshold", aggregate, "OccupancyThreshold")
            .AddAggregateProperty("CurrentOccupancyCount", aggregate, "CurrentOccupancyCount")
            .AddAggregateProperty("LastOccupancyReadingAtUtc", aggregate, "LastOccupancyReadingAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_EvacuationZones_Create]", parameters, cancellationToken: cancellationToken);

        await PersistOccupancyHistoryAsync(aggregate, connection, cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("EvacuationZoneId", aggregate, "EvacuationZoneId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ZoneCapacity", aggregate, "ZoneCapacity")
            .AddAggregateProperty("OccupancyThreshold", aggregate, "OccupancyThreshold")
            .AddAggregateProperty("CurrentOccupancyCount", aggregate, "CurrentOccupancyCount")
            .AddAggregateProperty("LastOccupancyReadingAtUtc", aggregate, "LastOccupancyReadingAtUtc")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_EvacuationZones_Update]", parameters, cancellationToken: cancellationToken);

        await PersistOccupancyHistoryAsync(aggregate, connection, cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    private async Task PersistOccupancyHistoryAsync(Agg aggregate, IDbConnection connection, CancellationToken cancellationToken)
    {
        foreach (var reading in aggregate.OccupancyReadings)
        {
            var parameters = new StoredProcedureBuilder()
                .AddGuid("OccupancyReadingId", reading.OccupancyReadingId.Value)
                .AddGuid("EvacuationZoneId", aggregate.EvacuationZoneId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddGuid("SiteId", aggregate.SiteId)
                .AddInt("HeadcountEstimate", reading.HeadcountEstimate)
                .AddString("Source", reading.Source.ToString(), 50)
                .AddDateTime("RecordedAtUtc", reading.RecordedAtUtc.Value)
                .AddBool("ExceededThreshold", reading.ExceededThreshold)
                .Build();

            await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_OccupancyReadings_Upsert]", parameters, cancellationToken: cancellationToken);
        }
    }

    /// <inheritdoc cref="IReadRepo.GetByIdAsync"/>
    public async Task<EvacuationZoneItemDto?> GetByIdAsync(Guid tenantId, Guid evacuationZoneId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("EvacuationZoneId", evacuationZoneId).Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<EvacuationZoneRow>(
            $"[{Schema}].[usp_EvacuationZones_GetById]", parameters, cancellationToken);

        return row is null || row.TenantId != tenantId ? null : ToDto(row);
    }

    /// <inheritdoc cref="IReadRepo.GetOccupancyByZoneAsync"/>
    public async Task<IReadOnlyList<OccupancyReadingItemDto>> GetOccupancyByZoneAsync(
        Guid tenantId, Guid? siteId, Guid zoneId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddGuid("EvacuationZoneId", zoneId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        var rows = await queryExecutor.QueryAsync<OccupancyReadingRow>(
            $"[{Schema}].[usp_OccupancyReadings_GetByZone]", parameters, cancellationToken);

        return rows.Select(r => new OccupancyReadingItemDto
        {
            OccupancyReadingId = r.OccupancyReadingId,
            EvacuationZoneId = r.EvacuationZoneId,
            HeadcountEstimate = r.HeadcountEstimate,
            Source = r.Source,
            RecordedAtUtc = r.RecordedAtUtc,
            ExceededThreshold = r.ExceededThreshold,
        }).ToList();
    }

    private static EvacuationZoneItemDto ToDto(EvacuationZoneRow row) => new()
    {
        EvacuationZoneId = row.EvacuationZoneId,
        TenantId = row.TenantId,
        SiteId = row.SiteId,
        ZoneName = row.ZoneName ?? string.Empty,
        TwinModelVersion = row.TwinModelVersion ?? string.Empty,
        ZoneCapacity = row.ZoneCapacity,
        OccupancyThreshold = row.OccupancyThreshold,
        CurrentOccupancyCount = row.CurrentOccupancyCount,
        LastOccupancyReadingAtUtc = row.LastOccupancyReadingAtUtc,
        ZoneStatus = row.Status ?? string.Empty,
        CreatedAtUtc = row.CreatedAtUtc ?? default,
        CreatedBy = row.CreatedBy,
    };
}
