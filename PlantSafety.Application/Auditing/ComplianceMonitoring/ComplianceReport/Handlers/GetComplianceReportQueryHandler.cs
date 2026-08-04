// AUTHOR: PlantSafety Platform Team (Security & Compliance Agent, Phase 9 §5.6 Compliance Report Generation)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for GetComplianceReportQuery. Composes the three real, previously
//   independent data sources via the SharedKernel IReportSectionProvider<,> cross-module
//   composition pattern (Handbook Part IX §9.3.2) -- resolved through ordinary DI, never dispatched
//   as another module's Query through ISender. Each section is retrieved independently: a failure
//   in one section degrades that section only (SectionAvailable=false, empty payload) rather than
//   failing the whole report, mirroring GenerateAuditReportCommandHandler's own
//   timeline-section-failure handling.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Contracts.ReadModelComposition;
using PlantSafety.Application.Auditing.ComplianceMonitoring.ComplianceReport.Models;
using PlantSafety.Application.Auditing.ComplianceMonitoring.ComplianceReport.Queries;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Contracts;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Models;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Queries;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.GlobalSafetyInsights.Contracts;
using PlantSafety.Application.GlobalSafetyInsights.Models;

namespace PlantSafety.Application.Auditing.ComplianceMonitoring.ComplianceReport.Handlers;

/// <summary>Handles <see cref="GetComplianceReportQuery"/>.</summary>
public sealed class GetComplianceReportQueryHandler(
    IReportSectionProvider<GlobalSafetyKpiSectionRequest, GlobalSafetyInsightsItemDto?> safetyKpiSectionProvider,
    IReportSectionProvider<TenantAuditStreamSectionRequest, TenantAuditStreamResult> auditTrailSectionProvider,
    IReportSectionProvider<CrossSiteActionLogSectionRequest, IReadOnlyList<CrossSiteActionLogEntryDto>> crossSiteActionsSectionProvider,
    ILogger<GetComplianceReportQueryHandler> logger)
    : IRequestHandler<GetComplianceReportQuery, Result<ComplianceReportDto>>
{
    /// <inheritdoc/>
    public async Task<Result<ComplianceReportDto>> Handle(GetComplianceReportQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var safetyKpiResult = await safetyKpiSectionProvider.GetSectionAsync(
                new GlobalSafetyKpiSectionRequest(request.TenantId, request.FromUtc, request.ToUtc), cancellationToken);
            if (!safetyKpiResult.IsSuccess)
                logger.LogWarning(
                    "GetComplianceReportQuery: GlobalSafetyKpi section unavailable for Tenant {TenantId}: [{ErrorCode}] {Message}",
                    request.TenantId, safetyKpiResult.Error.Code, safetyKpiResult.Error.Message);

            var auditTrailResult = await auditTrailSectionProvider.GetSectionAsync(
                new TenantAuditStreamSectionRequest(request.TenantId, request.FromUtc, request.ToUtc), cancellationToken);
            if (!auditTrailResult.IsSuccess)
                logger.LogWarning(
                    "GetComplianceReportQuery: TenantAuditStream section unavailable for Tenant {TenantId}: [{ErrorCode}] {Message}",
                    request.TenantId, auditTrailResult.Error.Code, auditTrailResult.Error.Message);

            var crossSiteActionsResult = await crossSiteActionsSectionProvider.GetSectionAsync(
                new CrossSiteActionLogSectionRequest(request.TenantId, request.FromUtc, request.ToUtc), cancellationToken);
            if (!crossSiteActionsResult.IsSuccess)
                logger.LogWarning(
                    "GetComplianceReportQuery: CrossSiteActionLog section unavailable for Tenant {TenantId}: [{ErrorCode}] {Message}",
                    request.TenantId, crossSiteActionsResult.Error.Code, crossSiteActionsResult.Error.Message);

            var report = new ComplianceReportDto(
                request.TenantId,
                request.FromUtc,
                request.ToUtc,
                DateTime.UtcNow,
                safetyKpiResult.IsSuccess ? safetyKpiResult.Value : null,
                safetyKpiResult.IsSuccess,
                auditTrailResult.IsSuccess ? auditTrailResult.Value.Entries : Array.Empty<PermissionUsageLogEntry>(),
                auditTrailResult.IsSuccess,
                crossSiteActionsResult.IsSuccess ? crossSiteActionsResult.Value : Array.Empty<CrossSiteActionLogEntryDto>(),
                crossSiteActionsResult.IsSuccess);

            return Result<ComplianceReportDto>.Success(report);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate compliance report for Tenant {TenantId}", request.TenantId);

            return Result<ComplianceReportDto>.Failure(new ApplicationError(
                "Auditing.ComplianceMonitoring.ComplianceReport.GenerationFailed",
                "Unable to generate the compliance report at this time.",
                Guid.NewGuid()));
        }
    }
}
