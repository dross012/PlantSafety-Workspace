// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.TsdfFacilities
//  Layer:          Infrastructure
//  File:           TsdfFacilityDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Dapper repository for the TsdfFacility aggregate root. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Specifications;
using PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.ValueObjects.Ids;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Aggregates.TsdfFacility;
using IWriteRepo = PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Interfaces.ITsdfFacilityRepository;

namespace PlantSafety.Infrastructure.HazardousWasteManagement.TsdfFacilities.Persistence;

/// <summary>Flat Dapper row for [HazardousWasteManagement].[TsdfFacilities], column-for-column with the TsdfFacility aggregate.</summary>
internal sealed class TsdfFacilityRow
{
    public Guid TsdfFacilityId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? FacilityName { get; init; }
    public string? EpaFacilityId { get; init; }
    public DateTime? PermitExpiresAtUtc { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>Dapper-backed repository for the TsdfFacility aggregate. Integrates with
/// Infrastructure.Integrations for TSDF EDI hand-off (future extension point).</summary>
public sealed class TsdfFacilityDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter)
    : IWriteRepo
{
    private const string Schema = "HazardousWasteManagement";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(TsdfFacilityId id, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TsdfFacilityId", id.Value).Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<TsdfFacilityRow>(
            $"[{Schema}].[usp_TsdfFacilities_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, TsdfFacilityRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(TsdfFacilitySpecification specification, CancellationToken cancellationToken = default)
    {
        var rows = await queryExecutor.QueryAsync<TsdfFacilityRow>(
            $"[{Schema}].[usp_TsdfFacilities_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows.Select(AggregateHydrator.Hydrate<Agg, TsdfFacilityRow>).Where(predicate).ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("TsdfFacilityId", aggregate, "TsdfFacilityId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("FacilityName", aggregate, "FacilityName")
            .AddAggregateProperty("EpaFacilityId", aggregate, "EpaFacilityId")
            .AddAggregateProperty("PermitExpiresAtUtc", aggregate, "PermitExpiresAtUtc")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_TsdfFacilities_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("TsdfFacilityId", aggregate, "TsdfFacilityId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_TsdfFacilities_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }
}
