// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetAnnualAuditQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.AnnualAudit.Queries;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.AnnualAudit.Repositories;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Deadlines.AnnualAudit.Handlers;

/// <summary>Handles <see cref="GetAnnualAuditQuery"/>.</summary>
public sealed class GetAnnualAuditQueryHandler : IRequestHandler<GetAnnualAuditQuery, Result<AnnualAuditQueryResult>>
{
    private readonly IAnnualAuditRepository _repository;
    private readonly ILogger<GetAnnualAuditQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAnnualAuditQueryHandler(IAnnualAuditRepository repository, ILogger<GetAnnualAuditQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AnnualAuditQueryResult>> Handle(GetAnnualAuditQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetDeadlinesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<AnnualAuditQueryResult>.Success(new AnnualAuditQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve AnnualAudit for Tenant {TenantId}", request.TenantId);

            return Result<AnnualAuditQueryResult>.Failure(new ApplicationError(
                "Compliance.RegulatoryCalendar.Deadlines.AnnualAudit.RetrievalFailed",
                "Unable to retrieve AnnualAudit data at this time.",
                Guid.NewGuid()));
        }
    }
}
