// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.EffluentSample
//  Layer:          Infrastructure
//  File:           EffluentSampleDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the EffluentSample aggregate. Chain-of-custody is
//                  immutable/append-only: UpdateAsync only ever changes Status (Corrected/
//                  Voided) -- all measurement fields are set once at insert. Additive-only.
// ============================================================================
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Specifications;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.ValueObjects.Ids;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Aggregates.EffluentSample;
using IWriteRepo = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Interfaces.IEffluentSampleRepository;
using IReadRepo = PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Repositories.IEffluentSampleRepository;

namespace PlantSafety.Infrastructure.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Persistence;

/// <summary>Flat Dapper row for [EnvironmentalHealth].[EffluentSample], column-for-column with the aggregate.</summary>
internal sealed class EffluentSampleRow
{
    public Guid EffluentSampleId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public Guid DischargePointId { get; init; }
    public string? Parameter { get; init; }
    public decimal MeasuredValue { get; init; }
    public string? UnitOfMeasure { get; init; }
    public DateTime SampledAtUtc { get; init; }
    public Guid CollectedByUserId { get; init; }
    public string? LabSourceSystem { get; init; }
    public string? ChainOfCustodyReference { get; init; }
    public Guid? CorrectionOfSampleId { get; init; }
    public string? CorrectionReason { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>Dapper-backed repository for the EffluentSample aggregate.</summary>
public sealed class EffluentSampleDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<EffluentSampleDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "EnvironmentalHealth";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(EffluentSampleId id, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("EffluentSampleId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<EffluentSampleRow>(
            $"[{Schema}].[usp_EffluentSample_GetById]", parameters, cancellationToken);
        return row is null ? null : AggregateHydrator.Hydrate<Agg, EffluentSampleRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(EffluentSampleSpecification specification, CancellationToken cancellationToken = default)
    {
        var rows = await queryExecutor.QueryAsync<EffluentSampleRow>(
            $"[{Schema}].[usp_EffluentSample_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);
        var predicate = specification.ToPredicate();
        return rows.Select(AggregateHydrator.Hydrate<Agg, EffluentSampleRow>).Where(predicate).ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("EffluentSampleId", aggregate, "EffluentSampleId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("DischargePointId", aggregate, "DischargePointId")
            .AddAggregateProperty("Parameter", aggregate, "Parameter")
            .AddAggregateProperty("MeasuredValue", aggregate, "MeasuredValue")
            .AddAggregateProperty("UnitOfMeasure", aggregate, "UnitOfMeasure")
            .AddAggregateProperty("SampledAtUtc", aggregate, "SampledAtUtc")
            .AddAggregateProperty("CollectedByUserId", aggregate, "CollectedByUserId")
            .AddAggregateProperty("LabSourceSystem", aggregate, "LabSourceSystem")
            .AddAggregateProperty("ChainOfCustodyReference", aggregate, "ChainOfCustodyReference")
            .AddAggregateProperty("CorrectionOfSampleId", aggregate, "CorrectionOfSampleId")
            .AddAggregateProperty("CorrectionReason", aggregate, "CorrectionReason")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_EffluentSample_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("EffluentSampleId", aggregate, "EffluentSampleId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_EffluentSample_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetSampleHistoryByPointAsync"/>
    public async Task<IReadOnlyList<EffluentSampleItemDto>> GetSampleHistoryByPointAsync(Guid tenantId, Guid dischargePointId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("DischargePointId", dischargePointId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EffluentSampleItemDto>($"[{Schema}].[usp_EffluentSample_GetItems]", parameters, cancellationToken);
    }
}