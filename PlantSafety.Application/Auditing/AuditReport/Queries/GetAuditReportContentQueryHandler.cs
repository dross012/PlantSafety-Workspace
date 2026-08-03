// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Handler for GetAuditReportContentQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Domain.Auditing.AuditReport.Enums;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Auditing.AuditReport.Repositories;

namespace PlantSafety.Application.Auditing.AuditReport.Queries;

/// <summary>Handles <see cref="GetAuditReportContentQuery"/>.</summary>
public sealed class GetAuditReportContentQueryHandler : IRequestHandler<GetAuditReportContentQuery, Result<AuditReportContentResult>>
{
    private readonly IAuditReportReadRepository _readRepository;
    private readonly IAuditReportContentStore _contentStore;
    private readonly ILogger<GetAuditReportContentQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAuditReportContentQueryHandler(
        IAuditReportReadRepository readRepository, IAuditReportContentStore contentStore, ILogger<GetAuditReportContentQueryHandler> logger)
    {
        _readRepository = readRepository;
        _contentStore = contentStore;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AuditReportContentResult>> Handle(GetAuditReportContentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var report = await _readRepository.GetByIdAsync(request.TenantId, request.AuditReportId, cancellationToken);
            if (report is null || report.RenderedContentReference is null)
            {
                return Result<AuditReportContentResult>.Failure(new ApplicationError(
                    "Auditing.AuditReport.ContentNotAvailable", "The requested audit report has no generated content available.", Guid.NewGuid()));
            }

            var content = await _contentStore.GetAsync(report.RenderedContentReference, cancellationToken);
            if (content is null)
            {
                return Result<AuditReportContentResult>.Failure(new ApplicationError(
                    "Auditing.AuditReport.ContentNotFound", "The rendered audit report content could not be located.", Guid.NewGuid()));
            }

            var format = AuditReportFormat.From(report.Format);
            var fileName = $"AuditReport-{report.TargetEntityType}-{report.TargetEntityId:N}.{format.Code.ToLowerInvariant()}";

            return Result<AuditReportContentResult>.Success(new AuditReportContentResult(format.ContentType, fileName, content));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve AuditReport content {AuditReportId} for Tenant {TenantId}", request.AuditReportId, request.TenantId);

            return Result<AuditReportContentResult>.Failure(new ApplicationError(
                "Auditing.AuditReport.ContentRetrievalFailed", "Unable to retrieve the audit report content at this time.", Guid.NewGuid()));
        }
    }
}
