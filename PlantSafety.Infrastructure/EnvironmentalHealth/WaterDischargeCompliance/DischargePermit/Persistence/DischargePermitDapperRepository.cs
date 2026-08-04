// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePermit
//  Layer:          Infrastructure
//  File:           DischargePermitDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the DischargePermit aggregate. Implements both the
//                  Domain-layer write repository (IDischargePermitRepository, GetByIdAsync/
//                  ListAsync/AddAsync/UpdateAsync) and the Application-layer read-model
//                  repository of the same simple name (GetDischargePermitByIdAsync). All DB
//                  access via stored procedures; no dynamic SQL. Aggregate rehydration uses
//                  AggregateHydrator (reflection, confined to Infrastructure). Additive-only.
// ============================================================================
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Specifications;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.ValueObjects.Ids;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Aggregates.DischargePermit;
using IWriteRepo = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Interfaces.IDischargePermitRepository;
using IReadRepo = PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Repositories.IDischargePermitRepository;

namespace PlantSafety.Infrastructure.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Persistence;

/// <summary>Flat Dapper row for [EnvironmentalHealth].[DischargePermit], column-for-column with the aggregate.</summary>
internal sealed class DischargePermitRow
{
    public Guid DischargePermitId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public string? PermitNumber { get; init; }
    public string? RegulatoryProgram { get; init; }
    public string? IssuingAuthority { get; init; }
    public DateTime EffectiveAtUtc { get; init; }
    public DateTime ExpiresAtUtc { get; init; }
    public int RenewalCount { get; init; }
    public string? PermitConditionSummary { get; init; }
    public string? LifecycleActionReason { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Dapper-backed repository for the DischargePermit aggregate. Implements the Domain write
/// repository and the Application read-model repository against the same table.
/// </summary>
public sealed class DischargePermitDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<DischargePermitDapperRepository> logger)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "EnvironmentalHealth";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(DischargePermitId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for DischargePermit {DischargePermitId}", id);

        var parameters = new StoredProcedureBuilder().AddGuid("DischargePermitId", id.Value).Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<DischargePermitRow>(
            $"[{Schema}].[usp_DischargePermit_GetById]", parameters, cancellationToken);

        return row is null ? null : AggregateHydrator.Hydrate<Agg, DischargePermitRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(DischargePermitSpecification specification, CancellationToken cancellationToken = default)
    {
        var rows = await queryExecutor.QueryAsync<DischargePermitRow>(
            $"[{Schema}].[usp_DischargePermit_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);

        var predicate = specification.ToPredicate();
        return rows.Select(AggregateHydrator.Hydrate<Agg, DischargePermitRow>).Where(predicate).ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("DischargePermitId", aggregate, "DischargePermitId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("PermitNumber", aggregate, "PermitNumber")
            .AddAggregateProperty("RegulatoryProgram", aggregate, "RegulatoryProgram")
            .AddAggregateProperty("IssuingAuthority", aggregate, "IssuingAuthority")
            .AddAggregateProperty("EffectiveAtUtc", aggregate, "EffectiveAtUtc")
            .AddAggregateProperty("ExpiresAtUtc", aggregate, "ExpiresAtUtc")
            .AddAggregateProperty("RenewalCount", aggregate, "RenewalCount")
            .AddAggregateProperty("PermitConditionSummary", aggregate, "PermitConditionSummary")
            .AddAggregateProperty("LifecycleActionReason", aggregate, "LifecycleActionReason")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_DischargePermit_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("DischargePermitId", aggregate, "DischargePermitId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("PermitNumber", aggregate, "PermitNumber")
            .AddAggregateProperty("RegulatoryProgram", aggregate, "RegulatoryProgram")
            .AddAggregateProperty("IssuingAuthority", aggregate, "IssuingAuthority")
            .AddAggregateProperty("EffectiveAtUtc", aggregate, "EffectiveAtUtc")
            .AddAggregateProperty("ExpiresAtUtc", aggregate, "ExpiresAtUtc")
            .AddAggregateProperty("RenewalCount", aggregate, "RenewalCount")
            .AddAggregateProperty("PermitConditionSummary", aggregate, "PermitConditionSummary")
            .AddAggregateProperty("LifecycleActionReason", aggregate, "LifecycleActionReason")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_DischargePermit_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetDischargePermitByIdAsync"/>
    public async Task<DischargePermitItemDto?> GetDischargePermitByIdAsync(Guid tenantId, Guid dischargePermitId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("DischargePermitId", dischargePermitId).Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<DischargePermitItemDto>(
            $"[{Schema}].[usp_DischargePermit_GetById]", parameters, cancellationToken);
    }
}