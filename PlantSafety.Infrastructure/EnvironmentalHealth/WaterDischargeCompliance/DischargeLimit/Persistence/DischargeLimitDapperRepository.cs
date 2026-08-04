// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit
//  Layer:          Infrastructure
//  File:           DischargeLimitDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the DischargeLimit aggregate. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Specifications;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.ValueObjects.Ids;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Aggregates.DischargeLimit;
using IWriteRepo = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Interfaces.IDischargeLimitRepository;
using IReadRepo = PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Repositories.IDischargeLimitRepository;

namespace PlantSafety.Infrastructure.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Persistence;

/// <summary>Flat Dapper row for [EnvironmentalHealth].[DischargeLimit], column-for-column with the aggregate.</summary>
internal sealed class DischargeLimitRow
{
    public Guid DischargeLimitId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public Guid DischargePointId { get; init; }
    public Guid DischargePermitId { get; init; }
    public string? Parameter { get; init; }
    public string? LimitBasis { get; init; }
    public decimal LimitValue { get; init; }
    public string? UnitOfMeasure { get; init; }
    public DateTime EffectiveAtUtc { get; init; }
    public string? PermitConditionReference { get; init; }
    public int RevisionCount { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>Dapper-backed repository for the DischargeLimit aggregate.</summary>
public sealed class DischargeLimitDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<DischargeLimitDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "EnvironmentalHealth";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(DischargeLimitId id, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("DischargeLimitId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<DischargeLimitRow>(
            $"[{Schema}].[usp_DischargeLimit_GetById]", parameters, cancellationToken);
        return row is null ? null : AggregateHydrator.Hydrate<Agg, DischargeLimitRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(DischargeLimitSpecification specification, CancellationToken cancellationToken = default)
    {
        var rows = await queryExecutor.QueryAsync<DischargeLimitRow>(
            $"[{Schema}].[usp_DischargeLimit_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);
        var predicate = specification.ToPredicate();
        return rows.Select(AggregateHydrator.Hydrate<Agg, DischargeLimitRow>).Where(predicate).ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("DischargeLimitId", aggregate, "DischargeLimitId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("DischargePointId", aggregate, "DischargePointId")
            .AddAggregateProperty("DischargePermitId", aggregate, "DischargePermitId")
            .AddAggregateProperty("Parameter", aggregate, "Parameter")
            .AddAggregateProperty("LimitBasis", aggregate, "LimitBasis")
            .AddAggregateProperty("LimitValue", aggregate, "LimitValue")
            .AddAggregateProperty("UnitOfMeasure", aggregate, "UnitOfMeasure")
            .AddAggregateProperty("EffectiveAtUtc", aggregate, "EffectiveAtUtc")
            .AddAggregateProperty("PermitConditionReference", aggregate, "PermitConditionReference")
            .AddAggregateProperty("RevisionCount", aggregate, "RevisionCount")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_DischargeLimit_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("DischargeLimitId", aggregate, "DischargeLimitId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("LimitValue", aggregate, "LimitValue")
            .AddAggregateProperty("RevisionCount", aggregate, "RevisionCount")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_DischargeLimit_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.ListDischargeLimitsByPointAsync"/>
    public async Task<IReadOnlyList<DischargeLimitItemDto>> ListDischargeLimitsByPointAsync(Guid tenantId, Guid dischargePointId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).AddGuid("DischargePointId", dischargePointId).Build();
        return await queryExecutor.QueryAsync<DischargeLimitItemDto>($"[{Schema}].[usp_DischargeLimit_GetItems]", parameters, cancellationToken);
    }
}