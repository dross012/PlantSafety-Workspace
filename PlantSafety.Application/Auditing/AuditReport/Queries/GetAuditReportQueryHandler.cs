// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Handler for GetAuditReportQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Auditing.AuditReport.Models;
using PlantSafety.Application.Auditing.AuditReport.Repositories;

namespace PlantSafety.Application.Auditing.AuditReport.Queries;

/// <summary>Handles <see cref="GetAuditReportQuery"/>.</summary>
public sealed class GetAuditReportQueryHandler : IRequestHandler<GetAuditReportQuery, Result<AuditReportDto>>
{
    private readonly IAuditReportReadRepository _repository;
    private readonly ILogger<GetAuditReportQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAuditReportQueryHandler(IAuditReportReadRepository repository, ILogger<GetAuditReportQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AuditReportDto>> Handle(GetAuditReportQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = await _repository.GetByIdAsync(request.TenantId, request.AuditReportId, cancellationToken);

            return dto is null
                ? Result<AuditReportDto>.Failure(new ApplicationError(
                    "Auditing.AuditReport.NotFound", "The requested audit report was not found.", Guid.NewGuid()))
                : Result<AuditReportDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve AuditReport {AuditReportId} for Tenant {TenantId}", request.AuditReportId, request.TenantId);

            return Result<AuditReportDto>.Failure(new ApplicationError(
                "Auditing.AuditReport.RetrievalFailed", "Unable to retrieve the audit report at this time.", Guid.NewGuid()));
        }
    }
}
