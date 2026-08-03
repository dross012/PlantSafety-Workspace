// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Handler for GetAuditReportHistoryQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Auditing.AuditReport.Models;
using PlantSafety.Application.Auditing.AuditReport.Repositories;

namespace PlantSafety.Application.Auditing.AuditReport.Queries;

/// <summary>Handles <see cref="GetAuditReportHistoryQuery"/>.</summary>
public sealed class GetAuditReportHistoryQueryHandler : IRequestHandler<GetAuditReportHistoryQuery, Result<IReadOnlyList<AuditReportDto>>>
{
    private readonly IAuditReportReadRepository _repository;
    private readonly ILogger<GetAuditReportHistoryQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAuditReportHistoryQueryHandler(IAuditReportReadRepository repository, ILogger<GetAuditReportHistoryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<AuditReportDto>>> Handle(GetAuditReportHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetForTargetAsync(request.TenantId, request.TargetEntityType, request.TargetEntityId, cancellationToken);
            return Result<IReadOnlyList<AuditReportDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve AuditReport history for {TargetEntityType} {TargetEntityId} Tenant {TenantId}",
                request.TargetEntityType, request.TargetEntityId, request.TenantId);

            return Result<IReadOnlyList<AuditReportDto>>.Failure(new ApplicationError(
                "Auditing.AuditReport.HistoryRetrievalFailed", "Unable to retrieve audit report history at this time.", Guid.NewGuid()));
        }
    }
}
