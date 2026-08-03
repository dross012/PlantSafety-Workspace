// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Explicit, deterministic static mapping between the AuditReport domain
//   aggregate and its Application-layer DTOs. See ComplianceEvaluationMapper remarks for why this
//   codebase uses explicit static mappers rather than Mapster/reflection mapping.
// SchemaVersion: 1

using PlantSafety.Domain.Auditing.AuditReport.Entities;
using PlantSafety.Application.Auditing.AuditReport.Models;
using AuditReportAggregate = PlantSafety.Domain.Auditing.AuditReport.Aggregates.AuditReport;

namespace PlantSafety.Application.Auditing.AuditReport.Mapping;

/// <summary>Explicit static mapper between AuditReport domain types and Application DTOs.</summary>
public static class AuditReportMapper
{
    /// <summary>Maps an <see cref="AuditReportAggregate"/> aggregate to its DTO representation.</summary>
    public static AuditReportDto ToDto(AuditReportAggregate report)
        => new()
        {
            AuditReportId = report.AuditReportId.Value,
            TenantId = report.TenantId,
            SiteId = report.SiteId,
            TargetEntityType = report.TargetEntityType,
            TargetEntityId = report.TargetEntityId,
            Format = report.Format.Code,
            Status = report.Status.Code,
            Sections = report.Sections.Select(ToDto).ToList(),
            ContentSizeBytes = report.ContentSizeBytes,
            RenderedContentReference = report.RenderedContentReference,
            FailureReason = report.FailureReason,
            RequestedBy = report.RequestedBy.Value,
            RequestedAtUtc = report.RequestedAtUtc.Value,
            CompletedAtUtc = report.CompletedAtUtc?.Value,
        };

    /// <summary>Maps an <see cref="AuditReportSection"/> entity to its DTO representation.</summary>
    public static AuditReportSectionDto ToDto(AuditReportSection section)
        => new()
        {
            SectionType = section.SectionType.Code,
            Heading = section.Heading,
            ContentJson = section.ContentJson,
        };
}
