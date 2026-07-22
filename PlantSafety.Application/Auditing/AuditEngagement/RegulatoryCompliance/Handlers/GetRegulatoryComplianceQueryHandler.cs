// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetRegulatoryComplianceQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Auditing.AuditEngagement.RegulatoryCompliance.Queries;
using PlantSafety.Application.Auditing.AuditEngagement.RegulatoryCompliance.Repositories;

namespace PlantSafety.Application.Auditing.AuditEngagement.RegulatoryCompliance.Handlers;

/// <summary>Handles <see cref="GetRegulatoryComplianceQuery"/>.</summary>
public sealed class GetRegulatoryComplianceQueryHandler : IRequestHandler<GetRegulatoryComplianceQuery, Result<RegulatoryComplianceQueryResult>>
{
    private readonly IRegulatoryComplianceRepository _repository;
    private readonly ILogger<GetRegulatoryComplianceQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRegulatoryComplianceQueryHandler(IRegulatoryComplianceRepository repository, ILogger<GetRegulatoryComplianceQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RegulatoryComplianceQueryResult>> Handle(GetRegulatoryComplianceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<RegulatoryComplianceQueryResult>.Success(new RegulatoryComplianceQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve RegulatoryCompliance for Tenant {TenantId}", request.TenantId);

            return Result<RegulatoryComplianceQueryResult>.Failure(new ApplicationError(
                "Auditing.AuditEngagement.RegulatoryCompliance.RetrievalFailed",
                "Unable to retrieve Regulatory Compliance Auditing data at this time.",
                Guid.NewGuid()));
        }
    }
}