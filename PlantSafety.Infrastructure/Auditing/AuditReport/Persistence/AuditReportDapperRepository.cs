// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Infrastructure
//  File:           AuditReportDapperRepository.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IAuditReportRepository. All DB access via
//                  stored procedures in the [Auditing] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator (mirrors RiskDapperRepository's precedent) because
//                  AuditReport exposes only a private rehydration constructor and private-setter
//                  properties. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Auditing.AuditReport.Entities;
using PlantSafety.Domain.Auditing.AuditReport.Enums;
using PlantSafety.Domain.Auditing.AuditReport.Interfaces;
using PlantSafety.Domain.Auditing.AuditReport.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
using AuditReportAggregate = PlantSafety.Domain.Auditing.AuditReport.Aggregates.AuditReport;

namespace PlantSafety.Infrastructure.Auditing.AuditReport.Persistence;

/// <summary>Dapper-backed repository for the <see cref="AuditReportAggregate"/> aggregate root.</summary>
public sealed class AuditReportDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<AuditReportDapperRepository> logger)
    : IAuditReportRepository
{
    /// <inheritdoc/>
    public async Task<AuditReportAggregate?> GetByIdAsync(AuditReportId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("AuditReportId", id.Value).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<ReportRow>("[Auditing].[usp_AuditReport_GetById]", parameters, cancellationToken);
        if (row is null) return null;

        var sectionRows = await GetSectionRowsAsync(id, cancellationToken);
        return Hydrate(row, sectionRows);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AuditReportAggregate>> GetForTargetAsync(Guid tenantId, string targetEntityType, Guid targetEntityId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddString("TargetEntityType", targetEntityType, 200)
            .AddGuid("TargetEntityId", targetEntityId)
            .Build();

        var rows = await queryExecutor.QueryAsync<ReportRow>("[Auditing].[usp_AuditReport_GetForTarget]", parameters, cancellationToken);

        var results = new List<AuditReportAggregate>();
        foreach (var row in rows)
        {
            var sectionRows = await GetSectionRowsAsync(AuditReportId.FromGuid(row.AuditReportId), cancellationToken);
            results.Add(Hydrate(row, sectionRows));
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(AuditReportAggregate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add AuditReport {AuditReportId}", aggregate.AuditReportId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var reportParams = new StoredProcedureBuilder()
            .AddGuid("AuditReportId", aggregate.AuditReportId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("TargetEntityType", aggregate.TargetEntityType, 200)
            .AddGuid("TargetEntityId", aggregate.TargetEntityId)
            .AddString("Format", aggregate.Format.Code, 10)
            .AddString("Status", aggregate.Status.Code, 20)
            .AddGuid("RequestedBy", aggregate.RequestedBy.Value)
            .AddDateTime("RequestedAtUtc", aggregate.RequestedAtUtc.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Auditing].[usp_AuditReport_InsertDraft]", reportParams, tx, cancellationToken: cancellationToken);

        foreach (var section in aggregate.Sections)
        {
            var sectionParams = new StoredProcedureBuilder()
                .AddGuid("AuditReportSectionId", section.AuditReportSectionId.Value)
                .AddGuid("AuditReportId", aggregate.AuditReportId.Value)
                .AddString("SectionType", section.SectionType.Code, 30)
                .AddInt("PrintOrder", section.SectionType.PrintOrder)
                .AddString("Heading", section.Heading, 200)
                .AddString("ContentJson", section.ContentJson, size: -1)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[Auditing].[usp_AuditReportSection_Insert]", sectionParams, tx, cancellationToken: cancellationToken);
        }

        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(AuditReportAggregate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update AuditReport {AuditReportId} outcome {Status}", aggregate.AuditReportId.Value, aggregate.Status.Code);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AuditReportId", aggregate.AuditReportId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.Code, 20)
            .AddLong("ContentSizeBytes", aggregate.ContentSizeBytes)
            .AddString("RenderedContentReference", aggregate.RenderedContentReference, 1000)
            .AddString("FailureReason", aggregate.FailureReason, 2000)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value ?? DateTime.UtcNow)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Auditing].[usp_AuditReport_UpdateOutcome]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        aggregate.ClearDomainEvents();
    }

    private async Task<IReadOnlyList<SectionRow>> GetSectionRowsAsync(AuditReportId id, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("AuditReportId", id.Value).Build();
        return await queryExecutor.QueryAsync<SectionRow>("[Auditing].[usp_AuditReportSection_GetByReportId]", parameters, cancellationToken);
    }

    private static AuditReportAggregate Hydrate(ReportRow row, IReadOnlyList<SectionRow> sectionRows)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<AuditReportAggregate>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(AuditReportAggregate.AuditReportId), AuditReportId.FromGuid(row.AuditReportId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(AuditReportAggregate.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AuditReportAggregate.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AuditReportAggregate.TargetEntityType), row.TargetEntityType);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AuditReportAggregate.TargetEntityId), row.TargetEntityId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AuditReportAggregate.Format), AuditReportFormat.From(row.Format));
        AggregateReflectionHydrator.SetProperty(agg, nameof(AuditReportAggregate.Status), AuditReportStatus.From(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(AuditReportAggregate.ContentSizeBytes), row.ContentSizeBytes);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AuditReportAggregate.RenderedContentReference), row.RenderedContentReference);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AuditReportAggregate.FailureReason), row.FailureReason);
        AggregateReflectionHydrator.SetProperty(agg, nameof(AuditReportAggregate.RequestedBy), new UserId(row.RequestedBy));
        AggregateReflectionHydrator.SetProperty(agg, nameof(AuditReportAggregate.RequestedAtUtc), UtcTimestamp.From(row.RequestedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(AuditReportAggregate.CompletedAtUtc), row.CompletedAtUtc.HasValue ? UtcTimestamp.From(row.CompletedAtUtc.Value) : (UtcTimestamp?)null);

        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<PlantSafety.Domain.Auditing.AuditReport.Events.IAuditReportDomainEvent>());
        AggregateReflectionHydrator.SetField(agg, "_sections", new List<AuditReportSection>());

        var sectionList = AggregateReflectionHydrator.GetPrivateCollectionField<AuditReportAggregate, List<AuditReportSection>>(agg, "_sections");
        foreach (var s in sectionRows.OrderBy(s => s.PrintOrder))
        {
            var child = AggregateReflectionHydrator.CreateUninitialized<AuditReportSection>();
            AggregateReflectionHydrator.SetProperty(child, nameof(AuditReportSection.AuditReportSectionId), AuditReportSectionId.FromGuid(s.AuditReportSectionId));
            AggregateReflectionHydrator.SetProperty(child, nameof(AuditReportSection.AuditReportId), AuditReportId.FromGuid(s.AuditReportId));
            AggregateReflectionHydrator.SetProperty(child, nameof(AuditReportSection.SectionType), AuditReportSectionType.From(s.SectionType));
            AggregateReflectionHydrator.SetProperty(child, nameof(AuditReportSection.Heading), s.Heading);
            AggregateReflectionHydrator.SetProperty(child, nameof(AuditReportSection.ContentJson), s.ContentJson);
            sectionList.Add(child);
        }

        return agg;
    }

    private sealed record ReportRow
    {
        public required Guid AuditReportId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string TargetEntityType { get; init; }
        public required Guid TargetEntityId { get; init; }
        public required string Format { get; init; }
        public required string Status { get; init; }
        public long? ContentSizeBytes { get; init; }
        public string? RenderedContentReference { get; init; }
        public string? FailureReason { get; init; }
        public required Guid RequestedBy { get; init; }
        public required DateTime RequestedAtUtc { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
    }

    private sealed record SectionRow
    {
        public required Guid AuditReportSectionId { get; init; }
        public required Guid AuditReportId { get; init; }
        public required string SectionType { get; init; }
        public required int PrintOrder { get; init; }
        public required string Heading { get; init; }
        public required string ContentJson { get; init; }
    }
}
