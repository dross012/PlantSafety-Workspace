// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Infrastructure
//  File:           AuditReportReadDapperRepository.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Application-layer AuditReport read model
//                  (IAuditReportReadRepository). Reuses the same stored procedures as the
//                  Domain-facing write repository but projects flat DTOs directly. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Application.Auditing.AuditReport.Models;
using PlantSafety.Application.Auditing.AuditReport.Repositories;

namespace PlantSafety.Infrastructure.Auditing.AuditReport.Persistence;

/// <summary>Dapper-backed repository for AuditReport read-model queries.</summary>
public sealed class AuditReportReadDapperRepository(DapperQueryExecutor queryExecutor, ILogger<AuditReportReadDapperRepository> logger) : IAuditReportReadRepository
{
    /// <inheritdoc/>
    public async Task<AuditReportDto?> GetByIdAsync(Guid tenantId, Guid auditReportId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById read-model for AuditReport {AuditReportId} Tenant {TenantId}", auditReportId, tenantId);

        var parameters = new StoredProcedureBuilder().AddGuid("AuditReportId", auditReportId).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<ReportRow>("[Auditing].[usp_AuditReport_GetById]", parameters, cancellationToken);
        if (row is null) return null;

        var sectionParameters = new StoredProcedureBuilder().AddGuid("AuditReportId", auditReportId).Build();
        var sectionRows = await queryExecutor.QueryAsync<SectionRow>("[Auditing].[usp_AuditReportSection_GetByReportId]", sectionParameters, cancellationToken);

        return ToDto(row, sectionRows);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AuditReportDto>> GetForTargetAsync(Guid tenantId, string targetEntityType, Guid targetEntityId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddString("TargetEntityType", targetEntityType, 200)
            .AddGuid("TargetEntityId", targetEntityId)
            .Build();

        var rows = await queryExecutor.QueryAsync<ReportRow>("[Auditing].[usp_AuditReport_GetForTarget]", parameters, cancellationToken);

        var results = new List<AuditReportDto>();
        foreach (var row in rows)
        {
            var sectionParameters = new StoredProcedureBuilder().AddGuid("AuditReportId", row.AuditReportId).Build();
            var sectionRows = await queryExecutor.QueryAsync<SectionRow>("[Auditing].[usp_AuditReportSection_GetByReportId]", sectionParameters, cancellationToken);
            results.Add(ToDto(row, sectionRows));
        }

        return results;
    }

    private static AuditReportDto ToDto(ReportRow row, IEnumerable<SectionRow> sectionRows)
        => new()
        {
            AuditReportId = row.AuditReportId,
            TenantId = row.TenantId,
            SiteId = row.SiteId,
            TargetEntityType = row.TargetEntityType,
            TargetEntityId = row.TargetEntityId,
            Format = row.Format,
            Status = row.Status,
            Sections = sectionRows.OrderBy(s => s.PrintOrder).Select(s => new AuditReportSectionDto
            {
                SectionType = s.SectionType,
                Heading = s.Heading,
                ContentJson = s.ContentJson,
            }).ToList(),
            ContentSizeBytes = row.ContentSizeBytes,
            RenderedContentReference = row.RenderedContentReference,
            FailureReason = row.FailureReason,
            RequestedBy = row.RequestedBy,
            RequestedAtUtc = row.RequestedAtUtc,
            CompletedAtUtc = row.CompletedAtUtc,
        };

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
        public required string SectionType { get; init; }
        public required int PrintOrder { get; init; }
        public required string Heading { get; init; }
        public required string ContentJson { get; init; }
    }
}
