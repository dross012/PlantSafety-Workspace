// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Infrastructure
//  File:           EsgDisclosureReportDapperRepository.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IEsgDisclosureReportRepository. AddAsync
//                  covers both the version-1 Generate() path and each superseding version's
//                  follow-on insert (a new row, never an update, per the module's immutability
//                  rule). UpdateAsync is status-only (Publish/Supersede transitions) and never
//                  rewrites a Published row's content fields. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Aggregates;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Enums;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Events;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Interfaces;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Specifications;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Sustainability.AdvancedMetrics.Persistence;

/// <summary>Dapper-backed repository for the EsgDisclosureReport aggregate root.</summary>
public sealed class EsgDisclosureReportDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter)
    : IEsgDisclosureReportRepository
{
    /// <inheritdoc/>
    public async Task<EsgDisclosureReport?> GetByIdAsync(EsgDisclosureReportId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("EsgDisclosureReportId", id.Value).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<ReportRow>(
            "[SustainabilityAdvancedMetrics].[usp_EsgDisclosureReport_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<EsgDisclosureReport>> ListAsync(EsgDisclosureReportSpecification specification, CancellationToken cancellationToken = default)
        => throw new NotSupportedException(
            "EsgDisclosureReportSpecification.ListAsync requires a TenantId-bearing specification variant; " +
            "use GetByIdAsync for the supported access path in this build-out.");

    /// <inheritdoc/>
    public async Task AddAsync(EsgDisclosureReport aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("EsgDisclosureReportId", aggregate.EsgDisclosureReportId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddGuid("SourceLedgerId", aggregate.SourceLedgerId.Value)
            .AddString("Frameworks", aggregate.Frameworks.ToString(), 100)
            .AddDateTime("ReportingPeriodStartUtc", aggregate.ReportingPeriodStartUtc.Value)
            .AddDateTime("ReportingPeriodEndUtc", aggregate.ReportingPeriodEndUtc.Value)
            .AddInt("Version", aggregate.Version)
            .AddGuid("SupersedesReportId", aggregate.SupersedesReportId?.Value)
            .AddString("MethodologyVersion", aggregate.MethodologyVersion, 100)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddDateTime("GeneratedAtUtc", aggregate.GeneratedAtUtc.Value)
            .AddGuid("GeneratedBy", aggregate.GeneratedBy.Value)
            .Build();

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        await commandExecutor.ExecuteAsync(conn, "[SustainabilityAdvancedMetrics].[usp_EsgDisclosureReport_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        DispatchDomainEvents(aggregate);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(EsgDisclosureReport aggregate, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("EsgDisclosureReportId", aggregate.EsgDisclosureReportId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddDateTime("PublishedAtUtc", aggregate.PublishedAtUtc?.Value)
            .Build();

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        await commandExecutor.ExecuteScalarAsync(conn, "[SustainabilityAdvancedMetrics].[usp_EsgDisclosureReport_UpdateStatus]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        DispatchDomainEvents(aggregate);
    }

    private static EsgDisclosureReport Hydrate(ReportRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<EsgDisclosureReport>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(EsgDisclosureReport.EsgDisclosureReportId), EsgDisclosureReportId.FromGuid(row.EsgDisclosureReportId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(EsgDisclosureReport.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(EsgDisclosureReport.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(EsgDisclosureReport.SourceLedgerId), AdvancedMetricsLedgerId.FromGuid(row.SourceLedgerId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(EsgDisclosureReport.Frameworks), ParseFrameworks(row.Frameworks));
        AggregateReflectionHydrator.SetProperty(agg, nameof(EsgDisclosureReport.ReportingPeriodStartUtc), UtcTimestamp.From(row.ReportingPeriodStartUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(EsgDisclosureReport.ReportingPeriodEndUtc), UtcTimestamp.From(row.ReportingPeriodEndUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(EsgDisclosureReport.Version), row.Version);
        AggregateReflectionHydrator.SetProperty(agg, nameof(EsgDisclosureReport.SupersedesReportId), row.SupersedesReportId.HasValue ? EsgDisclosureReportId.FromGuid(row.SupersedesReportId.Value) : (EsgDisclosureReportId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(EsgDisclosureReport.MethodologyVersion), row.MethodologyVersion);
        AggregateReflectionHydrator.SetProperty(agg, nameof(EsgDisclosureReport.Status), Enum.Parse<EsgDisclosureReportStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(EsgDisclosureReport.GeneratedAtUtc), UtcTimestamp.From(row.GeneratedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(EsgDisclosureReport.GeneratedBy), new UserId(row.GeneratedBy));
        AggregateReflectionHydrator.SetProperty(agg, nameof(EsgDisclosureReport.PublishedAtUtc), row.PublishedAtUtc.HasValue ? UtcTimestamp.From(row.PublishedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IAdvancedMetricsDomainEvent>());

        return agg;
    }

    private static EsgDisclosureFramework ParseFrameworks(string raw)
    {
        var result = EsgDisclosureFramework.None;
        foreach (var token in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (Enum.TryParse<EsgDisclosureFramework>(token, ignoreCase: true, out var parsed))
                result |= parsed;
        }
        return result;
    }

    private void DispatchDomainEvents(EsgDisclosureReport aggregate)
    {
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    private sealed record ReportRow
    {
        public required Guid EsgDisclosureReportId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required Guid SourceLedgerId { get; init; }
        public required string Frameworks { get; init; }
        public required DateTime ReportingPeriodStartUtc { get; init; }
        public required DateTime ReportingPeriodEndUtc { get; init; }
        public required int Version { get; init; }
        public Guid? SupersedesReportId { get; init; }
        public required string MethodologyVersion { get; init; }
        public required string Status { get; init; }
        public required DateTime GeneratedAtUtc { get; init; }
        public required Guid GeneratedBy { get; init; }
        public DateTime? PublishedAtUtc { get; init; }
    }
}
