// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetComplianceAuditQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Auditing.ComplianceAudit.Queries;
using PlantSafety.Application.Compliance.Auditing.ComplianceAudit.Repositories;

namespace PlantSafety.Application.Compliance.Auditing.ComplianceAudit.Handlers;

/// <summary>Handles <see cref="GetComplianceAuditQuery"/>.</summary>
public sealed class GetComplianceAuditQueryHandler : IRequestHandler<GetComplianceAuditQuery, Result<ComplianceAuditQueryResult>>
{
    private readonly IComplianceAuditRepository _repository;
    private readonly ILogger<GetComplianceAuditQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetComplianceAuditQueryHandler(IComplianceAuditRepository repository, ILogger<GetComplianceAuditQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ComplianceAuditQueryResult>> Handle(GetComplianceAuditQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetAuditsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ComplianceAuditQueryResult>.Success(new ComplianceAuditQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ComplianceAudit for Tenant {TenantId}", request.TenantId);

            return Result<ComplianceAuditQueryResult>.Failure(new ApplicationError(
                "Compliance.Auditing.ComplianceAudit.RetrievalFailed",
                "Unable to retrieve compliance audit data at this time.",
                Guid.NewGuid()));
        }
    }
}
