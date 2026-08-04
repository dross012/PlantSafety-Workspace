// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteStreams
//  Layer:          Infrastructure
//  File:           WasteStreamDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Dapper repository for the WasteStream aggregate. Implements both the
//                  Domain-layer write repository and the Application-layer read-model
//                  repository against the same table. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Specifications;
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.ValueObjects.Ids;
using PlantSafety.Application.HazardousWasteManagement.WasteStreams.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Aggregates.WasteStream;
using IWriteRepo = PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Interfaces.IWasteStreamRepository;
using IReadRepo = PlantSafety.Application.HazardousWasteManagement.WasteStreams.Repositories.IWasteStreamRepository;

namespace PlantSafety.Infrastructure.HazardousWasteManagement.WasteStreams.Persistence;

/// <summary>Flat Dapper row for [HazardousWasteManagement].[WasteStreams], column-for-column with the WasteStream aggregate.</summary>
internal sealed class WasteStreamRow
{
    public Guid WasteStreamId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? StreamName { get; init; }
    public string? EpaWasteCode { get; init; }
    public string? HazardClass { get; init; }
    public string? GeneratorEpaId { get; init; }
    public int CharacterizationVersion { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>Dapper-backed repository for the WasteStream aggregate.</summary>
public sealed class WasteStreamDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<WasteStreamDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "HazardousWasteManagement";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(WasteStreamId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for WasteStream {WasteStreamId}", id);

        var parameters = new StoredProcedureBuilder().AddGuid("WasteStreamId", id.Value).Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<WasteStreamRow>(
            $"[{Schema}].[usp_WasteStreams_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, WasteStreamRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(WasteStreamSpecification specification, CancellationToken cancellationToken = default)
    {
        var rows = await queryExecutor.QueryAsync<WasteStreamRow>(
            $"[{Schema}].[usp_WasteStreams_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows.Select(AggregateHydrator.Hydrate<Agg, WasteStreamRow>).Where(predicate).ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("AddAsync for WasteStream {WasteStreamId}", aggregate.WasteStreamId);

        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("WasteStreamId", aggregate, "WasteStreamId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("StreamName", aggregate, "StreamName")
            .AddAggregateProperty("EpaWasteCode", aggregate, "EpaWasteCode")
            .AddAggregateProperty("HazardClass", aggregate, "HazardClass")
            .AddAggregateProperty("GeneratorEpaId", aggregate, "GeneratorEpaId")
            .AddAggregateProperty("CharacterizationVersion", aggregate, "CharacterizationVersion")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_WasteStreams_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("UpdateAsync for WasteStream {WasteStreamId}", aggregate.WasteStreamId);

        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("WasteStreamId", aggregate, "WasteStreamId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("EpaWasteCode", aggregate, "EpaWasteCode")
            .AddAggregateProperty("HazardClass", aggregate, "HazardClass")
            .AddAggregateProperty("CharacterizationVersion", aggregate, "CharacterizationVersion")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_WasteStreams_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetWasteStreamByIdAsync"/>
    public async Task<WasteStreamItemDto?> GetWasteStreamByIdAsync(Guid tenantId, Guid wasteStreamId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("WasteStreamId", wasteStreamId).Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<WasteStreamRow>(
            $"[{Schema}].[usp_WasteStreams_GetById]", parameters, cancellationToken);

        if (row is null || row.TenantId != tenantId) return null;

        return new WasteStreamItemDto
        {
            WasteStreamId = row.WasteStreamId,
            TenantId = row.TenantId,
            SiteId = row.SiteId,
            StreamName = row.StreamName ?? string.Empty,
            EpaWasteCode = row.EpaWasteCode ?? string.Empty,
            HazardClass = row.HazardClass ?? string.Empty,
            GeneratorEpaId = row.GeneratorEpaId ?? string.Empty,
            CharacterizationVersion = row.CharacterizationVersion,
            Status = row.Status ?? string.Empty,
            CreatedAtUtc = row.CreatedAtUtc ?? default,
            CreatedBy = row.CreatedBy,
        };
    }
}
