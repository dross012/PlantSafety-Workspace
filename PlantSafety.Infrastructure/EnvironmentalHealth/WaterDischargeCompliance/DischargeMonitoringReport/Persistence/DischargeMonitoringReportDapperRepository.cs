// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport
//  Layer:          Infrastructure
//  File:           DischargeMonitoringReportDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the DischargeMonitoringReport aggregate. Additive-only.
// ============================================================================
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Specifications;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.ValueObjects.Ids;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Models;
using PlantSafety.Infrastructure.Common.Persistence;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Aggregates.DischargeMonitoringReport;
using IWriteRepo = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Interfaces.IDischargeMonitoringReportRepository;
using IReadRepo = PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Repositories.IDischargeMonitoringReportRepository;

namespace PlantSafety.Infrastructure.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Persistence;

/// <summary>Flat Dapper row for [EnvironmentalHealth].[DischargeMonitoringReport], column-for-column with the aggregate.</summary>
internal sealed class DischargeMonitoringReportRow
{
    public Guid DischargeMonitoringReportId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? SiteId { get; init; }
    public string? Status { get; init; }
    public Guid DischargePermitId { get; init; }
    public DateTime ReportingPeriodStartUtc { get; init; }
    public DateTime ReportingPeriodEndUtc { get; init; }
    public DateTime DueAtUtc { get; init; }
    public DateTime? SubmittedAtUtc { get; init; }
    public Guid? SubmittedByUserId { get; init; }
    public int SamplesWithinLimitCount { get; init; }
    public int TotalSamplesCount { get; init; }
    public DateTime? RetentionExpiresAtUtc { get; init; }
    public string? RejectionReason { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public Guid CreatedBy { get; init; }
}

/// <summary>Dapper-backed repository for the DischargeMonitoringReport aggregate.</summary>
public sealed class DischargeMonitoringReportDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter)
    : IWriteRepo, IReadRepo
{
    private const string Schema = "EnvironmentalHealth";

    /// <inheritdoc cref="IWriteRepo.GetByIdAsync"/>
    public async Task<Agg?> GetByIdAsync(DischargeMonitoringReportId id, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("DischargeMonitoringReportId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<DischargeMonitoringReportRow>(
            $"[{Schema}].[usp_DischargeMonitoringReport_GetById]", parameters, cancellationToken);
        return row is null ? null : AggregateHydrator.Hydrate<Agg, DischargeMonitoringReportRow>(row);
    }

    /// <inheritdoc cref="IWriteRepo.ListAsync"/>
    public async Task<IReadOnlyList<Agg>> ListAsync(DischargeMonitoringReportSpecification specification, CancellationToken cancellationToken = default)
    {
        var rows = await queryExecutor.QueryAsync<DischargeMonitoringReportRow>(
            $"[{Schema}].[usp_DischargeMonitoringReport_GetAll]", new StoredProcedureBuilder().Build(), cancellationToken);
        var predicate = specification.ToPredicate();
        return rows.Select(AggregateHydrator.Hydrate<Agg, DischargeMonitoringReportRow>).Where(predicate).ToList();
    }

    /// <inheritdoc cref="IWriteRepo.AddAsync"/>
    public async Task AddAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddAggregateProperty("DischargeMonitoringReportId", aggregate, "DischargeMonitoringReportId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("SiteId", aggregate, "SiteId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("DischargePermitId", aggregate, "DischargePermitId")
            .AddAggregateProperty("ReportingPeriodStartUtc", aggregate, "ReportingPeriodStartUtc")
            .AddAggregateProperty("ReportingPeriodEndUtc", aggregate, "ReportingPeriodEndUtc")
            .AddAggregateProperty("DueAtUtc", aggregate, "DueAtUtc")
            .AddAggregateProperty("SamplesWithinLimitCount", aggregate, "SamplesWithinLimitCount")
            .AddAggregateProperty("TotalSamplesCount", aggregate, "TotalSamplesCount")
            .AddAggregateProperty("CreatedAtUtc", aggregate, "CreatedAtUtc")
            .AddAggregateProperty("CreatedBy", aggregate, "CreatedBy")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_DischargeMonitoringReport_Create]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IWriteRepo.UpdateAsync"/>
    public async Task UpdateAsync(Agg aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddDateTime("NowUtc", DateTime.UtcNow)
            .AddAggregateProperty("DischargeMonitoringReportId", aggregate, "DischargeMonitoringReportId")
            .AddAggregateProperty("TenantId", aggregate, "TenantId")
            .AddAggregateProperty("Status", aggregate, "Status")
            .AddAggregateProperty("SubmittedAtUtc", aggregate, "SubmittedAtUtc")
            .AddAggregateProperty("SubmittedByUserId", aggregate, "SubmittedByUserId")
            .AddAggregateProperty("SamplesWithinLimitCount", aggregate, "SamplesWithinLimitCount")
            .AddAggregateProperty("TotalSamplesCount", aggregate, "TotalSamplesCount")
            .AddAggregateProperty("RetentionExpiresAtUtc", aggregate, "RetentionExpiresAtUtc")
            .AddAggregateProperty("RejectionReason", aggregate, "RejectionReason")
            .Build();

        using var connection = connectionFactory.CreateConnection();
        await commandExecutor.ExecuteAsync(connection, $"[{Schema}].[usp_DischargeMonitoringReport_Update]", parameters, cancellationToken: cancellationToken);

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc cref="IReadRepo.GetDischargeMonitoringReportsByPeriodAsync"/>
    public async Task<IReadOnlyList<DischargeMonitoringReportItemDto>> GetDischargeMonitoringReportsByPeriodAsync(Guid tenantId, Guid? siteId, DateTime periodStartUtc, DateTime periodEndUtc, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("PeriodStartUtc", periodStartUtc)
            .AddDateTime("PeriodEndUtc", periodEndUtc)
            .Build();

        return await queryExecutor.QueryAsync<DischargeMonitoringReportItemDto>($"[{Schema}].[usp_DischargeMonitoringReport_GetItems]", parameters, cancellationToken);
    }
}