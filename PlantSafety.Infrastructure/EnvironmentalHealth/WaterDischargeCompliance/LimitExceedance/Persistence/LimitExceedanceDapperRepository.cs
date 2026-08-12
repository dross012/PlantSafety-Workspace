// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
//  Layer:          Infrastructure
//  File:           LimitExceedanceDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the LimitExceedance aggregate. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Specifications;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.ValueObjects.Ids;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Aggregates.LimitExceedance;
using IWriteRepo = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Interfaces.ILimitExceedanceRepository;
using IReadRepo = PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Repositories.ILimitExceedanceRepository;

namespace PlantSafety.Infrastructure.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Persistence;

/// <summary>Flat Dapper row for [EnvironmentalHealth].[LimitExceedance], column-for-column with the aggregate.</summary>
internal sealed class LimitExceedanceRow
{
    public Guid LimitExceedanceId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public Guid DischargePointId { get; init; }
    public Guid EffluentSampleId { get; init; }
    public Guid DischargeLimitId { get; init; }
    public string? ParameterName { get; init; }
    public decimal MeasuredValue { get; init; }
    public decimal LimitValue { get; init; }
    public decimal ExceedancePercentage { get; init; }
    public string? Severity { get; init; }
    public DateTime DetectedAtUtc { get; init; }
    public int RepeatExceedanceCount { get; init; }
    public DateTime? ResponseInitiatedAtUtc { get; init; }
    public Guid? ResponseInitiatedByUserId { get; init; }
    public string? ResponseNotes { get; init; }
    public DateTime? ClosedAtUtc { get; init; }
    public Guid? ClosedByUserId { get; init; }
    public string? ResolutionNotes { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>Dapper-backed repository for the LimitExceedance aggregate.</summary>
public sealed class LimitExceedanceDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "EnvironmentalHealth";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(LimitExceedanceId id, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("LimitExceedanceId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<LimitExceedanceRow>(
            $"[{Schema}].[usp_LimitExceedance_GetById]", parameters, cancellationToken);
        return row is null ? null : AggregateHydrator.Hydrate<Agg, LimitExceedanceRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(LimitExceedanceSpecification specification, CancellationToken cancellationToken = default)
    {
        var rows = await queryExecutor.QueryAsync<LimitExceedanceRow>(
            $"[{Schema}].[usp_LimitExceedance_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);
        var predicate = specification.ToPredicate();
        return rows.Select(AggregateHydrator.Hydrate<Agg, LimitExceedanceRow>).Where(predicate).ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("LimitExceedanceId", aggregate, "LimitExceedanceId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("DischargePointId", aggregate, "DischargePointId")
            .AddAggregateProperty("EffluentSampleId", aggregate, "EffluentSampleId")
            .AddAggregateProperty("DischargeLimitId", aggregate, "DischargeLimitId")
            .AddAggregateProperty("ParameterName", aggregate, "ParameterName")
            .AddAggregateProperty("MeasuredValue", aggregate, "MeasuredValue")
            .AddAggregateProperty("LimitValue", aggregate, "LimitValue")
            .AddAggregateProperty("ExceedancePercentage", aggregate, "ExceedancePercentage")
            .AddAggregateProperty("Severity", aggregate, "Severity")
            .AddAggregateProperty("DetectedAtUtc", aggregate, "DetectedAtUtc")
            .AddAggregateProperty("RepeatExceedanceCount", aggregate, "RepeatExceedanceCount")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_LimitExceedance_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("LimitExceedanceId", aggregate, "LimitExceedanceId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("ResponseInitiatedAtUtc", aggregate, "ResponseInitiatedAtUtc")
            .AddAggregateProperty("ResponseInitiatedByUserId", aggregate, "ResponseInitiatedByUserId")
            .AddAggregateProperty("ResponseNotes", aggregate, "ResponseNotes")
            .AddAggregateProperty("ClosedAtUtc", aggregate, "ClosedAtUtc")
            .AddAggregateProperty("ClosedByUserId", aggregate, "ClosedByUserId")
            .AddAggregateProperty("ResolutionNotes", aggregate, "ResolutionNotes")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_LimitExceedance_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.ListActiveExceedancesAsync"/>
    public async Task<IReadOnlyList<LimitExceedanceItemDto>> ListActiveExceedancesAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).AddGuid("SiteId", siteId).Build();
        return await queryExecutor.QueryAsync<LimitExceedanceItemDto>($"[{Schema}].[usp_LimitExceedance_GetItems]", parameters, cancellationToken);
    }
}